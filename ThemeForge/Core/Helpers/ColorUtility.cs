using ThemeForge.Core.Records;
using System.Globalization;

namespace ThemeForge.Core.Helpers
{
    /// <summary>
    /// Утилиты для работы с HEX-цветами и конвертации RGB/HSL.
    /// </summary>
    public static class ColorUtility
    {
        /// <summary>
        /// Пробует нормализовать входную HEX-строку.
        /// Поддерживаются форматы: #RGB, #RRGGBB, #RRGGBBAA.
        /// Результат: #RRGGBB, если альфа равна FF, иначе #RRGGBBAA.
        /// </summary>
        /// <param name="input">Исходная строка цвета.</param>
        /// <param name="normalized">Нормализованная HEX-строка.</param>
        /// <returns><c>true</c>, если цвет распознан.</returns>
        public static bool TryNormalizeHex(string? input, out string normalized)
        {
            normalized = string.Empty;

            if (string.IsNullOrWhiteSpace(input)) return false;

            string hexBytes = input.Trim().ToUpperInvariant();

            if (hexBytes.StartsWith("#", StringComparison.Ordinal)) hexBytes = hexBytes[1..];

            if (hexBytes.Length == 3)
            {
                hexBytes = $"{hexBytes[0]}{hexBytes[0]}{hexBytes[1]}{hexBytes[1]}{hexBytes[2]}{hexBytes[2]}";
            }
            else if (hexBytes.Length != 6 && hexBytes.Length != 8) return false;

            foreach (char c in hexBytes)
            {
                if (!char.IsAsciiHexDigit(c)) return false;
            }

            byte r = ParseByte(hexBytes, 0);
            byte g = ParseByte(hexBytes, 2);
            byte b = ParseByte(hexBytes, 4);
            byte a = 255;

            if (hexBytes.Length == 8) a = ParseByte(hexBytes, 6);

            normalized = ToHex(a, r, g, b);

            return true;
        }

        /// <summary>
        /// Нормализует HEX-строку или бросает исключение.
        /// </summary>
        /// <param name="input">Исходная строка цвета.</param>
        /// <returns>Нормализованная HEX-строка.</returns>
        public static string NormalizeHex(string input)
        {
            if (!TryNormalizeHex(input, out var normalized))
            {
                throw new FormatException($"Некорректный цвет: '{input}'.");
            }

            return normalized;
        }

        /// <summary>
        /// Пробует разобрать цвет в ARGB-компоненты.
        /// </summary>
        public static bool TryParseArgb(string? hex, out byte a, out byte r, out byte g, out byte b)
        {
            a = r = g = b = 0;

            if (!TryNormalizeHex(hex, out var normalized)) return false;

            string hexBytes = normalized[1..];
            bool isParse = false;

            if (hexBytes.Length >= 6)
            {
                a = 255;
                r = ParseByte(hexBytes, 0);
                g = ParseByte(hexBytes, 2);
                b = ParseByte(hexBytes, 4);
                isParse = true;
            }

            if (hexBytes.Length == 8) a = ParseByte(hexBytes, 6);

            return isParse;
        }

        /// <summary>
        /// Собирает HEX-строку из компонентов.
        /// </summary>
        public static string ToHex(byte a, byte r, byte g, byte b) => a == 255 ? $"#{r:X2}{g:X2}{b:X2}" : $"#{r:X2}{g:X2}{b:X2}{a:X2}";

        /// <summary>
        /// Конвертирует HEX в HSL.
        /// </summary>
        public static HslColor FromHex(string hex)
        {
            if (!TryParseArgb(hex, out var a, out var r, out var g, out var b))
            {
                throw new FormatException($"Некорректный цвет: '{hex}'.");
            }

            return FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Конвертирует RGBA в HSL.
        /// </summary>
        public static HslColor FromArgb(byte a, byte r, byte g, byte b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));

            double h = 0;
            double s = 0;
            double l = (max + min) / 2.0;

            if (Math.Abs(max - min) > double.Epsilon)
            {
                double d = max - min;

                s = d / l > 0.5 ? (2.0 - max - min) : (max + min);

                if (Math.Abs(max - rd) < double.Epsilon)
                {
                    h = (gd - bd) / d + (gd < bd ? 6.0 : 0.0);
                }
                else if (Math.Abs(max - gd) < double.Epsilon)
                {
                    h = (bd - rd) / d + 2.0;
                }
                else
                {
                    h = (rd - gd) / d + 4.0;
                }

                h *= 60.0;
            }

            return new HslColor(h, s, l, a / 255.0);
        }

        /// <summary>
        /// Конвертирует HSL в HEX.
        /// </summary>
        public static string ToHex(this in HslColor hsl)
        {
            var (a, r, g, b) = ToArgb(hsl);

            return ToHex(a, r, g, b);
        }

        /// <summary>
        /// Конвертирует HSL в RGBA-компоненты.
        /// </summary>
        public static (byte A, byte R, byte G, byte B) ToArgb(this in HslColor hsl)
        {
            double h = ((hsl.H % 360.0) + 360.0) % 360.0 / 360.0;
            double s = Clamp01(hsl.S);
            double l = Clamp01(hsl.L);
            double a = Clamp01(hsl.A);

            if (s <= double.Epsilon)
            {
                byte gray = ToByte(l);

                return (ToByte(a), gray, gray, gray);
            }

            double q = l < 0.5 ? l * (1.0 + s) : l + s - (l * s);
            double p = (2.0 * l) - q;

            double r = HueToRgb(p, q, h + (1.0 / 3.0));
            double g = HueToRgb(p, q, h);
            double b = HueToRgb(p, q, h - (1.0 / 3.0));

            return (ToByte(a), ToByte(r), ToByte(g), ToByte(b));
        }

        /// <summary>
        /// Евклидово расстояние между двумя цветами в RGB-пространстве.
        /// </summary>
        public static double GetRgbDistance(string hexA, string hexB)
        {
            if (!TryParseArgb(hexA, out var a1, out var r1, out var g1, out var b1) ||
                !TryParseArgb(hexB, out var a2, out var r2, out var g2, out var b2) || a1 != a2)
            {
                return double.MaxValue;
            }

            double dr = r1 - r2;
            double dg = g1 - g2;
            double db = b1 - b2;

            return Math.Sqrt((dr * dr) + (dg * dg) + (db * db));
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;

            if (t < 1.0 / 6.0) return p + ((q - p) * 6.0 * t);
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + ((q - p) * ((2.0 / 3.0) - t) * 6.0);

            return p;
        }

        private static byte ParseByte(string s, int offset) => byte.Parse(s.AsSpan(offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        private static byte ToByte(double value) => (byte)Math.Clamp((int)Math.Round(value * 255.0), 0, 255);

        private static double Clamp01(double value) => Math.Clamp(value, 0.0, 1.0);
    }
}
