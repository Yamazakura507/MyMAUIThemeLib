using Core.Services;
using Core.DI;

namespace Maui.DI
{
    /// <summary>
    /// Расширения <see cref="MauiAppBuilder"/> для ThemeForge.
    /// </summary>
    public static class ThemeForgeMauiAppBuilderExtensions
    {
        /// <summary>
        /// Подключает только MAUI-слой ThemeForge.
        /// </summary>
        public static MauiAppBuilder UseThemeForgeMaui(this MauiAppBuilder builder)
        {
            builder.Services.AddThemeForgeMaui();
            return builder;
        }

        /// <summary>
        /// Подключает ядро и MAUI-слой ThemeForge.
        /// </summary>
        public static MauiAppBuilder UseThemeForge(this MauiAppBuilder builder, Action<ThemeForgeCoreOptions>? configureCore = null)
        {
            builder.Services.AddThemeForgeCore(configureCore);
            builder.Services.AddThemeForgeMaui();

            return builder;
        }
    }
}
