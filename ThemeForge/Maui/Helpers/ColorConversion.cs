using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfColors.Gradients;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Core.Helpers;
using ThemeForge.Core.Records;
using GradientStop = ThemeForge.Abstractions.Records.UseOfColors.Gradients.GradientStop;

namespace ThemeForge.Maui.Helpers
{
    /// <summary>
    /// Утилиты конвертации HEX/градиентов в MAUI Color/Brush.
    /// </summary>
    public static class ColorConversion
    {
        private const string BlackHex = "#000000";
        private const string WhiteHex = "#FFFFFF";

        /// <summary>
        /// Конвертирует HEX-цвет в MAUI <see cref="Color"/>.
        /// </summary>
        /// <param name="hex">HEX-цвет.</param>
        public static Color ToColor(string? hex)
        {
            if (!ColorUtility.TryParseArgb(hex, out byte a, out byte r, out byte g, out byte b))
            {
                return Colors.Black;
            }

            return Color.FromRgba(r, g, b, a / 255f);
        }

        /// <summary>
        /// Конвертирует HEX-цвет в <see cref="SolidColorBrush"/>.
        /// </summary>
        /// <param name="hex">HEX-цвет.</param>
        public static SolidColorBrush ToSolidBrush(string? hex) => new SolidColorBrush(ToColor(hex));

        /// <summary>
        /// Конвертирует градиентную тему в MAUI <see cref="Brush"/>.
        /// </summary>
        /// <param name="gradient">Градиентная тема.</param>
        public static Brush ToBrush(this GradientTheme? gradient)
        {
            if (gradient is null || gradient.Stops.Count == 0)
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            if (gradient.Stops.Count == 1)
            {
                return ToSolidBrush(gradient.Stops[0].Color.Hex);
            }

            return gradient.Type switch
            {
                GradientType.Linear => ToLinearBrush(gradient),
                GradientType.Radial => ToRadialBrush(gradient),
                _ => ToSolidBrush(gradient.Stops[0].Color.Hex)
            };
        }

        /// <summary>
        /// Возвращает контрастный цвет для текста поверх указанного HEX-цвета.
        /// </summary>
        /// <param name="hex">Фоновый HEX-цвет.</param>
        public static string GetContrastHex(string hex)
        {
            if (!ColorUtility.TryParseArgb(hex, out _, out byte r, out byte g, out byte b))
            {
                return WhiteHex;
            }

            double luminance = ((0.299 * r) + (0.587 * g) + (0.114 * b)) / 255.0;

            return luminance > 0.605 ? BlackHex : WhiteHex;
        }

        /// <summary>
        /// Осветляет цвет, изменяя светлоту в HSL.
        /// </summary>
        /// <param name="hex">Исходный HEX-цвет.</param>
        /// <param name="amount">Величина осветления от -1 до 1.</param>
        public static string Lighten(string hex, double amount)
        {
            HslColor hsl = ColorUtility.FromHex(hex);

            hsl = hsl with { L = Math.Clamp(hsl.L + amount, 0.0, 1.0) };

            return hsl.ToHex();
        }

        /// <summary>
        /// Затемняет цвет, изменяя светлоту в HSL.
        /// </summary>
        /// <param name="hex">Исходный HEX-цвет.</param>
        /// <param name="amount">Величина затемнения от 0 до 1.</param>
        public static string Darken(string hex, double amount) => Lighten(hex, -amount);

        /// <summary>
        /// Смешивает два цвета в RGB-пространстве.
        /// </summary>
        /// <param name="hexA">Первый цвет.</param>
        /// <param name="hexB">Второй цвет.</param>
        /// <param name="weightA">Вес первого цвета от 0 до 1.</param>
        public static string Mix(string hexA, string hexB, double weightA)
        {
            weightA = Math.Clamp(weightA, 0.0, 1.0);
            double weightB = 1.0 - weightA;

            if (!ColorUtility.TryParseArgb(hexA, out byte a1, out byte r1, out byte g1, out byte b1) ||
                !ColorUtility.TryParseArgb(hexB, out byte a2, out byte r2, out byte g2, out byte b2))
            {
                return hexA;
            }

            byte a = ToByte((a1 * weightA) + (a2 * weightB));
            byte r = ToByte((r1 * weightA) + (r2 * weightB));
            byte g = ToByte((g1 * weightA) + (g2 * weightB));
            byte b = ToByte((b1 * weightA) + (b2 * weightB));

            return ColorUtility.ToHex(a, r, g, b);
        }

        private static Brush ToLinearBrush(GradientTheme gradient)
        {
            GradientGeometry geometry = gradient.Geometry;

            Point startPoint = geometry.StartPoint.HasValue ? ToPoint(geometry.StartPoint.Value) : ComputeLinearStart(geometry.AngleDegrees);
            Point endPoint = geometry.EndPoint.HasValue ? ToPoint(geometry.EndPoint.Value) : ComputeLinearEnd(geometry.AngleDegrees);

            LinearGradientBrush brush = new ()
            {
                StartPoint = startPoint,
                EndPoint = endPoint
            };

            AddStops(brush.GradientStops, gradient);

            return brush;
        }

        private static Brush ToRadialBrush(GradientTheme gradient)
        {
            GradientGeometry geometry = gradient.Geometry;

            Point center = geometry.CenterPoint.HasValue ? ToPoint(geometry.CenterPoint.Value) : new Point(0.5, 0.5);

            float radius = (float)Math.Clamp(geometry.Radius, 0.01, 10.0);

            RadialGradientBrush brush = new ()
            {
                Center = center,
                Radius = radius
            };

            AddStops(brush.GradientStops, gradient);

            return brush;
        }

        private static void AddStops(GradientStopCollection stops, GradientTheme gradient)
        {
            foreach (GradientStop stop in gradient.Stops)
            {
                stops.Add(new Microsoft.Maui.Controls.GradientStop(ToColor(stop.Color.Hex), (float)stop.Offset));
            }
        }

        private static Point ToPoint(NormalizedPoint point) => new Point(point.X, point.Y);

        private static Point ComputeLinearStart(double angleDegrees)
        {
            (double dx, double dy) = AngleToVector(angleDegrees);

            return new Point(0.5 - (dx / 2.0), 0.5 - (dy / 2.0));
        }

        private static Point ComputeLinearEnd(double angleDegrees)
        {
            (double dx, double dy) = AngleToVector(angleDegrees);

            return new Point(0.5 + (dx / 2.0), 0.5 + (dy / 2.0));
        }

        private static (double Dx, double Dy) AngleToVector(double angleDegrees)
        {
            double radians = angleDegrees * Math.PI / 180.0;

            return (Math.Cos(radians), Math.Sin(radians));
        }

        private static byte ToByte(double value) => (byte)Math.Clamp((int)Math.Round(value), 0, 255);
    }
}
