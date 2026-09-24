using System.Globalization;

namespace ThemeForge.Maui.Converters
{
    /// <summary>
    /// Инвертирует булево значение.
    /// </summary>
    public sealed class InverseBooleanConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b ? !b : false;
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b ? !b : false;
        }
    }
}
