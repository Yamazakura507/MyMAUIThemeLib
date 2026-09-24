using Abstractions.Interfaces;
using Maui.Services;

namespace Maui.DI
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
            services.AddSingleton<ThemeResourceBuilder>();
            services.AddSingleton<IThemeApplier, MauiThemeApplier>();
            services.AddSingleton<ThemeForgeMauiCoordinator>();

            return services;
        }
    }
}
