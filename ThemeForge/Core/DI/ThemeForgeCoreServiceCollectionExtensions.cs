using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Core.Interfaces;
using ThemeForge.Core.Services;
using ThemeForge.Core.Services.Name;
using ThemeForge.Core.Services.Storage;
using ThemeForge.Core.Services.Theme;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ThemeForge.Core.DI
{
    /// <summary>
    /// DI-расширения для ThemeForge.Core.
    /// </summary>
    public static class ThemeForgeCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует ядро ThemeForge.
        /// </summary>
        public static IServiceCollection AddThemeForgeCore(this IServiceCollection services, Action<ThemeForgeCoreOptions>? configure = null)
        {
            services.AddOptions<ThemeForgeCoreOptions>().Configure(configure ?? (_ => { }));

            services.AddSingleton<LocalColorNameProvider>();
            services.AddSingleton<IColorNameLookup>(sp => sp.GetRequiredService<LocalColorNameProvider>());
            services.AddSingleton<LocalColorNamingService>();
            services.AddSingleton<IColorNamingService>(sp => sp.GetRequiredService<LocalColorNamingService>());
            services.AddSingleton<LocalColorHarmonyService>();
            services.AddSingleton<IColorHarmonyService>(sp => sp.GetRequiredService<LocalColorHarmonyService>());
            services.AddSingleton<IThemeNamingService, ThemeNamingService>();
            services.AddSingleton<IControlThemeCatalog, DefaultControlThemeCatalog>();
            services.AddSingleton<IThemeFactory, ThemeFactory>();
            services.AddSingleton<IThemeFileStorage>(sp =>
            {
                ThemeForgeCoreOptions options = sp.GetRequiredService<IOptions<ThemeForgeCoreOptions>>().Value;

                if (string.IsNullOrWhiteSpace(options.StorageDirectory)) return new InMemoryThemeFileStorage();

                string path = Path.Combine(options.StorageDirectory, options.StorageFileName);

                return new FileSystemThemeFileStorage(path);
            });
            services.AddSingleton<IThemeRepository>(sp =>
            {
                IThemeFileStorage storage = sp.GetRequiredService<IThemeFileStorage>();
                ThemeForgeCoreOptions options = sp.GetRequiredService<IOptions<ThemeForgeCoreOptions>>().Value;

                return new JsonThemeRepository(storage, options.SeedBuiltInThemes);
            });
            services.AddSingleton<IThemeService, ThemeService>();

            return services;
        }
    }
}
