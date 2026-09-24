using ThemeForge.Abstractions.Enums;
using ThemeForge.Maui.Controls.Converters;

namespace ThemeForge.Maui.Controls.Helpers
{
    /// <summary>
    /// Вспомогательные значения видимости для типов градиента.
    /// </summary>
    public static class GradientTypeVisibility
    {
        /// <summary>
        /// Конвертер для проверки Linear.
        /// </summary>
        public static IValueConverter IsLinear { get; } = new EqualToConverter(GradientType.Linear);

        /// <summary>
        /// Конвертер для проверки Radial.
        /// </summary>
        public static IValueConverter IsRadial { get; } = new EqualToConverter(GradientType.Radial);
    }
}
