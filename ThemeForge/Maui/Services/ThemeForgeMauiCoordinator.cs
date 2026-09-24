using ThemeForge.Abstractions.EventArgs;
using ThemeForge.Abstractions.Interfaces;

namespace ThemeForge.Maui.Services
{
    /// <summary>
    /// Связывает <see cref="IThemeService"/> и <see cref="IThemeApplier"/>.
    /// </summary>
    public sealed class ThemeForgeMauiCoordinator : IDisposable
    {
        private readonly IThemeService themeService;
        private readonly IThemeApplier themeApplier;
        private bool initialized;

        /// <summary>
        /// Создает координатор MAUI-темы.
        /// </summary>
        public ThemeForgeMauiCoordinator(IThemeService themeService, IThemeApplier themeApplier)
        {
            this.themeService = themeService;
            this.themeApplier = themeApplier;
        }

        /// <summary>
        /// Инициализирует подписку и применяет текущую тему.
        /// </summary>
        public void Initialize()
        {
            if (initialized) return;

            initialized = true;

            themeService.Applied += OnThemeApplied;
            themeApplier.Apply(themeService.Current);
        }

        private void OnThemeApplied(object? sender, ThemeChangedEventArgs e)
        {
            if (e.IsApplied)
            {
                themeApplier.Apply(e.Theme);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!initialized) return;

            themeService.Applied -= OnThemeApplied;
            initialized = false;
        }
    }
}
