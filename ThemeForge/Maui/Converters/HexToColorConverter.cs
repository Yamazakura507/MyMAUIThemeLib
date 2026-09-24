using ThemeForge.Maui.Helpers;
using System.Globalization;

namespace ThemeForge.Maui.Converters
{
    /// <summary>
    /// Конвертирует HEX-строку в MAUI Color.
    /// </summary>
    public sealed class HexToColorConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return ColorConversion.ToColor(value as string);
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
