using ThemeForge.Maui.Controls.ComponentModels.Editors;
using ThemeForge.Maui.Controls.Interfaces;

namespace ThemeForge.Maui.Controls.DI
{
    /// <summary>
    /// DI-расширения для ThemeForge.Maui.Controls.
    /// </summary>
    public static class ThemeForgeMauiControlsServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует UI-контролы ThemeForge.
        /// </summary>
        public static IServiceCollection AddThemeForgeMauiControls(this IServiceCollection services)
        {
            services.AddSingleton<IFontCatalogService, SystemFontCatalogService>();

            return services;
        }
    }
}
