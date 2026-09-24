using Abstractions.Records.UseOfTheme;
using Maui.Helpers;
using System.Globalization;

namespace Maui.Converters
{
    /// <summary>
    /// Конвертирует <see cref="GradientTheme"/> в MAUI Brush.
    /// </summary>
    public sealed class GradientThemeToBrushConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is GradientTheme gradient ? gradient.ToBrush() : new SolidColorBrush(Colors.Transparent);
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
