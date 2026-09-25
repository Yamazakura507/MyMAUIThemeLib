using System.Globalization;

namespace ThemeForge.Maui.Studio.Converters
{
    /// <summary>
    /// Сравнивает значение enum со строковым параметром.
    /// </summary>
    public sealed class EnumToBoolConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || parameter is null) return false;

            string? valueText = value.ToString();
            string? parameterText = parameter.ToString();

            return string.Equals(valueText, parameterText, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
