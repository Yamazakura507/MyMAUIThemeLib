using System.Globalization;

namespace ThemeForge.Maui.Controls.Converters
{
    /// <summary>
    /// Конвертирует булево значение раскрытия узла в стрелку дерева.
    /// </summary>
    public sealed class BoolToArrowConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? "▼" : "▶";
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
