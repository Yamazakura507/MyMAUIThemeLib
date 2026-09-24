using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Abstractions.EventArgs
{
    /// <summary>
    /// Аргументы изменения темы.
    /// </summary>
    public sealed class ThemeChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Тема, к которой относится событие.
        /// </summary>
        public ThemeDefinition Theme { get; }

        /// <summary>
        /// True, если тема применена к интерфейсу. False, если изменен только draft/preview.
        /// </summary>
        public bool IsApplied { get; }

        /// <summary>
        /// Создает аргументы события.
        /// </summary>
        public ThemeChangedEventArgs(ThemeDefinition theme, bool isApplied)
        {
            Theme = theme;
            IsApplied = isApplied;
        }
    }
}
