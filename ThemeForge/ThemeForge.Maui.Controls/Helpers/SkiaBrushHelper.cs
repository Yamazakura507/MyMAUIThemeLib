using SkiaSharp;

namespace ThemeForge.Maui.Controls.Helpers
{
    /// <summary>
    /// Помощник для конвертации MAUI Brush в SkiaSharp shader.
    /// </summary>
    internal static class SkiaBrushHelper
    {
        /// <summary>
        /// Создает SKShader из MAUI Brush.
        /// </summary>
        /// <param name="brush">Исходный brush.</param>
        /// <param name="size">Размер области отрисовки.</param>
        /// <param name="tileMode">Режим повторения градиента.</param>
        public static SKShader? CreateShader(Brush? brush, SKSize size, SKShaderTileMode tileMode = SKShaderTileMode.Clamp)
        {
            if (brush is null || size.Width <= 0 || size.Height <= 0) return null;

            return brush switch
            {
                SolidColorBrush solid => CreateSolidShader(solid),
                LinearGradientBrush linear => CreateLinearShader(linear, size, tileMode),
                RadialGradientBrush radial => CreateRadialShader(radial, size, tileMode),
                _ => null
            };
        }

        private static SKShader? CreateSolidShader(SolidColorBrush brush) => SKShader.CreateColor(ToSkColor(brush.Color));

        private static SKShader? CreateLinearShader(LinearGradientBrush brush, SKSize size, SKShaderTileMode tileMode)
        {
            if (brush.GradientStops.Count == 0) return null;

            if (brush.GradientStops.Count == 1)
            {
                return SKShader.CreateColor(ToSkColor(brush.GradientStops[0].Color));
            }

            SKPoint start = ToSkPoint(brush.StartPoint, size);
            SKPoint end = ToSkPoint(brush.EndPoint, size);

            SKColor[] colors = new SKColor[brush.GradientStops.Count];
            float[] positions = new float[brush.GradientStops.Count];

            for (int i = 0; i < brush.GradientStops.Count; i++)
            {
                colors[i] = ToSkColor(brush.GradientStops[i].Color);
                positions[i] = Math.Clamp(brush.GradientStops[i].Offset, 0f, 1f);
            }

            return SKShader.CreateLinearGradient(start, end, colors, positions, tileMode);
        }

        private static SKShader? CreateRadialShader(RadialGradientBrush brush, SKSize size, SKShaderTileMode tileMode)
        {
            if (brush.GradientStops.Count == 0) return null;

            if (brush.GradientStops.Count == 1)
            {
                return SKShader.CreateColor(ToSkColor(brush.GradientStops[0].Color));
            }

            SKPoint center = ToSkPoint(brush.Center, size);

            float minSide = Math.Min(size.Width, size.Height);
            float radius = Math.Max(1f, (float)(brush.Radius * minSide));

            SKColor[] colors = new SKColor[brush.GradientStops.Count];
            float[] positions = new float[brush.GradientStops.Count];

            for (int i = 0; i < brush.GradientStops.Count; i++)
            {
                colors[i] = ToSkColor(brush.GradientStops[i].Color);
                positions[i] = Math.Clamp(brush.GradientStops[i].Offset, 0f, 1f);
            }

            return SKShader.CreateRadialGradient(center, radius, colors, positions, tileMode);
        }

        private static SKPoint ToSkPoint(Point point, SKSize size) => new SKPoint((float)(point.X * size.Width), (float)(point.Y * size.Height));

        private static SKColor ToSkColor(Color color) => new SKColor(ToChannel(color.Red), ToChannel(color.Green), ToChannel(color.Blue), ToChannel(color.Alpha));

        private static byte ToChannel(double value) => (byte)Math.Clamp(value * 255.0, 0.0, 255.0);
    }
}
