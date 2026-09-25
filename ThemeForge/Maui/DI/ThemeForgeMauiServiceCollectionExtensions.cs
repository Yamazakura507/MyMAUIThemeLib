using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Maui.Services;

namespace ThemeForge.Maui.DI
{
    /// <summary>
    /// DI-расширения для ThemeForge.Maui.
    /// </summary>
    public static class ThemeForgeMauiServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует MAUI-слой ThemeForge.
        /// </summary>
        public static IServiceCollection AddThemeForgeMaui(this IServiceCollection services)
        {
            services.AddSingleton(sp => new ThemeResourceBuilder(sp.GetService<IControlThemeCatalog>()));
            services.AddSingleton<IThemeApplier, MauiThemeApplier>();
            services.AddSingleton<ThemeForgeMauiCoordinator>();

            return services;
        }
    }
}
