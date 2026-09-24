using Maui.Helpers;
using System.Globalization;

namespace Maui.Converters
{
    /// <summary>
    /// Конвертирует HEX-строку в SolidColorBrush.
    /// </summary>
    public sealed class HexToBrushConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return ColorConversion.ToSolidBrush(value as string);
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
