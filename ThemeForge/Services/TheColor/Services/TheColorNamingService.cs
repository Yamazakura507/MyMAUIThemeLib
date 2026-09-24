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

namespace ThemeForge.Services.TheColor.Services
{
    /// <summary>
    /// Сервис именования цветов через TheColor API с локальным fallback и кэшированием.
    /// </summary>
    public sealed class TheColorNamingService : IColorNamingService
    {
        private readonly HttpClient httpClient;
        private readonly IColorNameLookup localLookup;
        private readonly IMemoryCache cache;
        private readonly TheColorApiOptions options;
        private readonly ConcurrentDictionary<string, Task<ColorToken>> inflightRequests = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Создает сервис именования цветов.
        /// </summary>
        public TheColorNamingService(HttpClient httpClient, IColorNameLookup localLookup, IMemoryCache cache, IOptions<TheColorApiOptions> options)
        {
            this.httpClient = httpClient;
            this.localLookup = localLookup;
            this.cache = cache;
            this.options = options.Value;
        }

        /// <inheritdoc />
        public async ValueTask<ColorToken> GetNameAsync(string hex, CancellationToken cancellationToken = default)
        {
            if (!ColorUtility.TryNormalizeHex(hex, out string normalized))
            {
                return ColorToken.FromHex(hex);
            }

            string cacheKey = GetCacheKey(normalized);

            if (cache.TryGetValue(cacheKey, out ColorToken? cached) && cached is not null)
            {
                return cached;
            }

            Task<ColorToken> task = inflightRequests.GetOrAdd(cacheKey, _ => FetchNameAsync(cacheKey, normalized, cancellationToken));

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

        /// <inheritdoc />
        public async ValueTask<IReadOnlyDictionary<string, ColorToken>> GetNamesAsync(IEnumerable<string> hexes, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(hexes);

            List<string> unique = new ();
            HashSet<string> seen = new (StringComparer.OrdinalIgnoreCase);

            foreach (string hex in hexes)
            {
                if (ColorUtility.TryNormalizeHex(hex, out string normalized) && seen.Add(normalized))
                {
                    unique.Add(normalized);
                }
            }

            if (unique.Count == 0)
            {
                return new Dictionary<string, ColorToken>(StringComparer.OrdinalIgnoreCase);
            }

            int parallelism = Math.Max(1, options.MaxParallelRequests);

            using SemaphoreSlim semaphore = new (parallelism, parallelism);

            IEnumerable<Task<(string, ColorToken)>> tasks = unique.Select(async normalized =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    ColorToken token = await GetNameAsync(normalized, cancellationToken).ConfigureAwait(false);

                    return (Hex: normalized, Token: token);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            (string, ColorToken)[] results = await Task.WhenAll(tasks).ConfigureAwait(false);

            Dictionary<string, ColorToken> dictionary = new (StringComparer.OrdinalIgnoreCase);

            foreach ((string Hex, ColorToken Token) result in results)
            {
                dictionary[result.Hex] = result.Token;
            }

            return dictionary;
        }

        private async Task<ColorToken> FetchNameAsync(string cacheKey, string normalizedHex, CancellationToken cancellationToken)
        {
            ColorToken result;

            if (options.EnableApi)
            {
                ColorToken? apiToken = await TryFetchFromApiAsync(normalizedHex, cancellationToken).ConfigureAwait(false);

                if (apiToken is not null)
                {
                    result = apiToken;
                }
                else
                {
                    result = CreateLocalToken(normalizedHex);
                }
            }
            else
            {
                result = CreateLocalToken(normalizedHex);
            }

            SetCache(cacheKey, result);

            return result;
        }

        private async Task<ColorToken?> TryFetchFromApiAsync(string normalizedHex, CancellationToken cancellationToken)
        {
            string hexWithoutHash = normalizedHex.TrimStart('#');
            string requestUri = $"hex/{hexWithoutHash}?format=json";

            using HttpResponseMessage? response = await SendWithRetryAsync(requestUri, cancellationToken).ConfigureAwait(false);

            if (response is null) return null;

            try
            {
                using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

                JsonElement root = document.RootElement;

                string? name = null;

                if (root.TryGetProperty("name", out JsonElement nameElement))
                {
                    name = nameElement.GetNameOrNull();
                }

                if (string.IsNullOrWhiteSpace(name)) return null;

                return new ColorToken
                {
                    Hex = normalizedHex,
                    Name = name,
                    Source = ColorNameSource.Api
                };
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

        private ColorToken CreateLocalToken(string normalizedHex)
        {
            string? name = localLookup.Lookup(normalizedHex);

            return new ColorToken
            {
                Hex = normalizedHex,
                Name = name,
                Source = name is null ? ColorNameSource.Hex : ColorNameSource.Local
            };
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

                    if (response.IsSuccessStatusCode) return response;

                    int statusCode = (int)response.StatusCode;

                    response.Dispose();

                    if (statusCode is >= (int)HttpStatusCode.BadRequest and < (int)HttpStatusCode.InternalServerError) return null;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Таймаут запроса. Повторяем, если остались попытки.
                }
                catch (HttpRequestException)
                {
                    // Сетевая ошибка. Повторяем, если остались попытки.
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

        private void SetCache(string key, ColorToken value)
        {
            if (options.CacheDuration <= TimeSpan.Zero) return;

            cache.Set(key, value, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = options.CacheDuration });
        }

        private static string GetCacheKey(string normalizedHex) => $"ThemeForge.TheColor.Name:{normalizedHex}";
    }
}
