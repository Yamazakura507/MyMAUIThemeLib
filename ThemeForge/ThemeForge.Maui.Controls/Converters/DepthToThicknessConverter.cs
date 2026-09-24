using System.Globalization;

namespace ThemeForge.Maui.Controls.Converters
{
    /// <summary>
    /// Превращает глубину узла дерева в левый отступ.
    /// </summary>
    public sealed class DepthToThicknessConverter : IValueConverter
    {
        private const double DefaultIndent = 14.0;

        /// <inheritdoc />
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            int depth = value is int i ? Math.Max(0, i) : 0;
            double indent = parameter is double d ? d : DefaultIndent;

            return new Thickness(depth * indent, 0, 0, 0);
        }

        /// <inheritdoc />
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
