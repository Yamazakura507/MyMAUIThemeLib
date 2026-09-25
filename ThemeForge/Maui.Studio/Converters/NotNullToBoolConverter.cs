using System.Globalization;

namespace ThemeForge.Maui.Studio.Converters
{
    /// <summary>
    /// Возвращает <c>true</c>, если объект не равен <c>null</c>.
    /// </summary>
    public sealed class NotNullToBoolConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is not null;
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
