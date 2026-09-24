using System.Globalization;

namespace ThemeForge.Maui.Controls.Converters
{
    internal sealed class EqualToConverter : IValueConverter
    {
        private readonly object value;

        public EqualToConverter(object value)
        {
            this.value = value;
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Equals(value, this.value);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
