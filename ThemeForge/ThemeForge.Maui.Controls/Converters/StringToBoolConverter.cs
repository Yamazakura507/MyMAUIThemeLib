using System.Globalization;

namespace ThemeForge.Maui.Controls.Converters
{
    /// <summary>
    /// Возвращает <c>true</c>, если строка не пустая.
    /// </summary>
    public sealed class StringToBoolConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
