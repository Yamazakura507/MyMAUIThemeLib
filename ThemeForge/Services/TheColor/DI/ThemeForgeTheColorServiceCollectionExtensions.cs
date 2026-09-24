using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ThemeForge.Services.TheColor.Helpers;
using ThemeForge.Services.TheColor.Services;

namespace ThemeForge.Services.TheColor.DI
{
    /// <summary>
    /// DI-расширения для подключения TheColor API.
    /// </summary>
    public static class ThemeForgeTheColorServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует сетевые адаптеры TheColor API.
        /// Вызывать после <c>AddThemeForgeCore()</c>.
        /// </summary>
        public static IServiceCollection AddThemeForgeTheColorApi(this IServiceCollection services, IConfiguration? configuration = null)
        {
            services.AddMemoryCache();
            services.AddOptions<TheColorApiOptions>();

            if (configuration is not null)
            {
                services.Configure<TheColorApiOptions>(configuration);
            }

            return AddThemeForgeTheColorApi(services);
        }

        /// <summary>
        /// Регистрирует сетевые адаптеры TheColor API с явной настройкой.
        /// </summary>
        public static IServiceCollection AddThemeForgeTheColorApi(this IServiceCollection services, Action<TheColorApiOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            services.AddMemoryCache();
            services.Configure(configure);

            return AddThemeForgeTheColorApi(services);
        }

        private static IServiceCollection AddThemeForgeTheColorApi(this IServiceCollection services)
        {
            services.AddHttpClient(TheColorHttpClientNames.Default,
                (sp, client) =>
                {
                    TheColorApiOptions options = sp.GetRequiredService<IOptions<TheColorApiOptions>>().Value;

                    ConfigureHttpClient(client, options);
                });

            services.Replace(
                ServiceDescriptor.Singleton<IColorNamingService>(sp =>
                {
                    FillServiceParam(sp, out HttpClient httpClient, out IColorNameLookup localLookup, out IMemoryCache cache, out IOptions<TheColorApiOptions> options);

                    return new TheColorNamingService(httpClient, localLookup, cache, options);
                }));
            services.Replace(
                ServiceDescriptor.Singleton<IColorHarmonyService>(sp =>
                {
                    FillServiceParam(sp, out HttpClient httpClient, out IColorNameLookup localLookup, out IMemoryCache cache, out IOptions<TheColorApiOptions> options);

                    return new TheColorHarmonyService(httpClient, localLookup, cache, options);
                }));

            return services;
        }

        private static void FillServiceParam(IServiceProvider sp, out HttpClient httpClient, out IColorNameLookup localLookup, out IMemoryCache cache, out IOptions<TheColorApiOptions> options)
        {
            IHttpClientFactory httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            httpClient = httpClientFactory.CreateClient(TheColorHttpClientNames.Default);

            localLookup = sp.GetRequiredService<IColorNameLookup>();
            cache = sp.GetRequiredService<IMemoryCache>();
            options = sp.GetRequiredService<IOptions<TheColorApiOptions>>();
        }

        private static void ConfigureHttpClient(HttpClient client, TheColorApiOptions options)
        {
            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
            else
            {
                client.BaseAddress = new Uri("https://www.thecolorapi.com/id");
            }

            // Таймаут управляется на уровне отдельного запроса, чтобы retry работал корректно.
            client.Timeout = Timeout.InfiniteTimeSpan;

            foreach (KeyValuePair<string,string> header in options.DefaultHeaders)
            {
                if (string.IsNullOrWhiteSpace(header.Key) || header.Value is null) continue;

                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}
