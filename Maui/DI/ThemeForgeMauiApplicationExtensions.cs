using Maui.Services;

namespace Maui.DI
{
    /// <summary>
    /// Расширения для инициализации ThemeForge в MAUI-приложении.
    /// </summary>
    public static class ThemeForgeMauiApplicationExtensions
    {
        /// <summary>
        /// Инициализирует применение темы к подключенному приложению.
        /// Вызывайте после того, как у <see cref="Application"/> появился handler/MAUI context.
        /// </summary>
        public static void InitializeThemeForge(this Application application)
        {
            ArgumentNullException.ThrowIfNull(application);

            IServiceProvider? services = application.Handler?.MauiContext?.Services;

            if (services is null) return;

            ThemeForgeMauiCoordinator coordinator = services.GetRequiredService<ThemeForgeMauiCoordinator>();

            coordinator.Initialize();
        }
    }
}
