using ThemeForge.Maui.Controls.ComponentModels.Editors;
using ThemeForge.Maui.Controls.DI;
using ThemeForge.Maui.Studio.ComponentModels;
using ThemeForge.Maui.Studio.ComponentModels.Editors;
using ThemeForge.Maui.Studio.Pages;

namespace ThemeForge.Maui.Studio.DI
{
    /// <summary>
    /// DI-расширения для ThemeForge.Maui.Studio.
    /// </summary>
    public static class ThemeForgeMauiStudioServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует центральную студию тем.
        /// </summary>
        public static IServiceCollection AddThemeForgeMauiStudio(this IServiceCollection services)
        {
            services.AddThemeForgeMauiControls();

            services.AddSingleton<SolidThemeEditorViewModel>();
            services.AddSingleton<GradientThemeEditorViewModel>();
            services.AddSingleton<TypographyEditorViewModel>();
            services.AddSingleton<GeometryEditorViewModel>();
            services.AddSingleton<ComponentThemeEditorViewModel>();

            services.AddSingleton<ThemeStudioViewModel>();
            services.AddTransient<ThemeStudioPage>();

            return services;
        }
    }
}
