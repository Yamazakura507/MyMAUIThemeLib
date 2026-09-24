using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Core.Helpers;
using ThemeForge.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using ThemeForge.Services.TheColor.Helpers;
using ThemeForge.Services.TheColor.Records;

namespace ThemeForge.Services.TheColor.Services
{
    /// <summary>
    /// Сервис цветовых гармоний через TheColor API с локальным fallback и кэшированием.
    /// </summary>
    public sealed class TheColorHarmonyService : IColorHarmonyService
    {
        private readonly HttpClient httpClient;
        private readonly IColorNameLookup localLookup;
        private readonly IMemoryCache cache;
        private readonly TheColorApiOptions options;
        private readonly ConcurrentDictionary<string, Task<IReadOnlyList<ColorToken>>> inflightRequests = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Создает сервис гармоний.
        /// </summary>
        public TheColorHarmonyService(HttpClient httpClient, IColorNameLookup localLookup, IMemoryCache cache, IOptions<TheColorApiOptions> options)
        {
            this.httpClient = httpClient;
            this.localLookup = localLookup;
            this.cache = cache;
            this.options = options.Value;
        }

        /// <inheritdoc />
        public async ValueTask<IReadOnlyList<ColorToken>> GenerateAsync(ColorToken baseColor, ColorHarmonyMode mode, int count = 5, CancellationToken cancellationToken = default)
        {
            if (count <= 0) return [];

            ArgumentNullException.ThrowIfNull(baseColor);

            if (!ColorUtility.TryNormalizeHex(baseColor.Hex, out string normalizedHex)) return [];

            int safeCount = Math.Clamp(count, 1, 12);
            string cacheKey = GetCacheKey(normalizedHex, mode, safeCount);

            if (cache.TryGetValue(cacheKey, out IReadOnlyList<ColorToken>? cached) && cached is not null)
            {
                return cached;
            }

            Task<IReadOnlyList<ColorToken>> task = inflightRequests.GetOrAdd(cacheKey, _ => FetchAsync(cacheKey, normalizedHex, mode, safeCount, cancellationToken));

            try
            {
                return await task.ConfigureAwait(false);
            }
            finally
            {
                if (task.IsCompleted)
                {
                    inflightRequests.TryRemove(cacheKey, out _);
                }
            }
        }

        private async Task<IReadOnlyList<ColorToken>> FetchAsync(string cacheKey, string normalizedHex, ColorHarmonyMode mode, int count, CancellationToken cancellationToken)
        {
            IReadOnlyList<ColorToken> result;

            if (options.EnableApi && count >= 2)
            {
                IReadOnlyList<ColorToken>? apiResult = await TryFetchFromApiAsync(normalizedHex, mode, count, cancellationToken).ConfigureAwait(false);

                result = apiResult is { Count: > 0 } ? apiResult : CreateLocalPalette(normalizedHex, mode, count);
            }
            else
            {
                result = CreateLocalPalette(normalizedHex, mode, count);
            }

            SetCache(cacheKey, result);
            return result;
        }

        private async Task<IReadOnlyList<ColorToken>?> TryFetchFromApiAsync(string normalizedHex, ColorHarmonyMode mode, int count, CancellationToken cancellationToken)
        {
            string hexWithoutHash = normalizedHex.TrimStart('#');
            string harmonyType = GetApiHarmonyType(mode);
            string requestUri = $"harmony?hex={hexWithoutHash}&type={harmonyType}&count={count}&format=json";

            using HttpResponseMessage? response = await SendWithRetryAsync(requestUri, cancellationToken).ConfigureAwait(false);

            if (response is null) return null;

            try
            {
                using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (!document.RootElement.TryGetColorsArray(out JsonElement colors)) return null;

                List<ParsedColor> parsed = new ();

                foreach (JsonElement colorElement in colors.EnumerateArray())
                {
                    string? hex = colorElement.GetHexOrNull();

                    if (hex is null) continue;

                    string? name = null;

                    if (colorElement.ValueKind == JsonValueKind.Object && colorElement.TryGetProperty("name", out JsonElement nameElement))
                    {
                        name = nameElement.GetNameOrNull();
                    }

                    parsed.Add(new ParsedColor(hex, name));

                    if (parsed.Count >= count) break;
                }

                if (parsed.Count == 0) return null;

                List<ColorToken> result = new (parsed.Count);

                foreach (ParsedColor item in parsed)
                {
                    string? finalName = item.Name;
                    ColorNameSource source = ColorNameSource.Hex;

                    if (!string.IsNullOrWhiteSpace(finalName))
                    {
                        source = ColorNameSource.Api;
                    }
                    else
                    {
                        finalName = localLookup.Lookup(item.Hex);

                        if (!string.IsNullOrWhiteSpace(finalName))
                        {
                            source = ColorNameSource.Local;
                        }
                    }

                    result.Add(new ColorToken
                    {
                        Hex = item.Hex,
                        Name = finalName,
                        Source = source
                    });
                }

                return result;
            }
            catch (JsonException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        private IReadOnlyList<ColorToken> CreateLocalPalette(string normalizedHex, ColorHarmonyMode mode, int count)
        {
            IReadOnlyList<ColorToken> palette = ColorHarmonyGenerator.Generate(normalizedHex, mode, count);
            List<ColorToken> result = new (palette.Count);

            foreach (ColorToken token in palette)
            {
                string? name = localLookup.Lookup(token.Hex);

                result.Add(name is null ? token : token with { Name = name, Source = ColorNameSource.Local });
            }

            return result;
        }

        private async Task<HttpResponseMessage?> SendWithRetryAsync(string requestUri, CancellationToken cancellationToken)
        {
            int attempts = Math.Max(1, options.RetryCount + 1);

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                try
                {
                    using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds)));

                    HttpResponseMessage response = await httpClient.GetAsync(requestUri, timeoutCts.Token).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    int statusCode = (int)response.StatusCode;
                    response.Dispose();

                    if (statusCode is >= (int)HttpStatusCode.BadRequest and < (int)HttpStatusCode.InternalServerError) return null;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Таймаут запроса.
                }
                catch (HttpRequestException)
                {
                    // Сетевая ошибка.
                }

                if (attempt == attempts - 1) return null;

                int delayMs = (int)(200 * Math.Pow(2, attempt));

                try
                {
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }

            return null;
        }

        private void SetCache(string key, IReadOnlyList<ColorToken> value)
        {
            if (options.CacheDuration <= TimeSpan.Zero) return;

            cache.Set(key, value, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = options.CacheDuration });
        }

        private static string GetApiHarmonyType(ColorHarmonyMode mode) => mode switch
        {
            ColorHarmonyMode.Monochrome => "monochrome",
            ColorHarmonyMode.Analogous => "analogous",
            ColorHarmonyMode.Complementary => "complementary",
            ColorHarmonyMode.Triadic => "triadic",
            ColorHarmonyMode.Tetradic => "tetradic",
            _ => mode.ToString().ToLowerInvariant()
        };

        private static string GetCacheKey(string normalizedHex, ColorHarmonyMode mode, int count) => $"ThemeForge.TheColor.Harmony:{normalizedHex}:{mode}:{count}";
    }
}
