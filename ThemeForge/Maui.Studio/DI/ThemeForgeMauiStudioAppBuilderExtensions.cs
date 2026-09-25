namespace ThemeForge.Maui.Studio.DI
{
    // <summary>
    /// Расширения <see cref="MauiAppBuilder"/> для студии тем.
    /// </summary>
    public static class ThemeForgeMauiStudioAppBuilderExtensions
    {
        /// <summary>
        /// Подключает студию тем.
        /// </summary>
        public static MauiAppBuilder UseThemeForgeStudio(this MauiAppBuilder builder)
        {
            builder.Services.AddThemeForgeMauiStudio();

            return builder;
        }
    }
}
