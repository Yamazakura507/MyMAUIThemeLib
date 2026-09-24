using Microsoft.Extensions.Configuration;
using ThemeForge.Services.TheColor.Services;
using ThemeForge.Services.TheColor.DI;

namespace ThemeForge.Services.Maui.TheColor.DI
{
    /// <summary>
    /// Расширения MauiAppBuilder для TheColor API.
    /// </summary>
    public static class ThemeForgeTheColorMauiBuilderExtensions
    {
        /// <summary>
        /// Подключает TheColor API как обогащающий слой.
        /// </summary>
        public static MauiAppBuilder UseThemeForgeTheColorApi(this MauiAppBuilder builder, IConfigurationSection? section = null)
        {
            builder.Services.AddThemeForgeTheColorApi(section);

            return builder;
        }

        /// <summary>
        /// Подключает TheColor API с программной настройкой.
        /// </summary>
        public static MauiAppBuilder UseThemeForgeTheColorApi(this MauiAppBuilder builder, Action<TheColorApiOptions> configure)
        {
            builder.Services.AddThemeForgeTheColorApi(configure);

            return builder;
        }
    }
}
