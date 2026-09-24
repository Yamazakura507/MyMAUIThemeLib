using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Core.Records;

namespace ThemeForge.Core.Helpers
{
    /// <summary>
    /// Локальный генератор цветовых гармоний.
    /// </summary>
    public static class ColorHarmonyGenerator
    {
        private static readonly int maxHarmonyCount = 12;

        /// <summary>
        /// Генерирует палитру на основе HEX-цвета.
        /// </summary>
        public static IReadOnlyList<ColorToken> Generate(string baseHex, ColorHarmonyMode mode, int count = 5)
        {
            HslColor hsl = ColorUtility.FromHex(baseHex);

            return Generate(hsl, mode, count);
        }

        /// <summary>
        /// Генерирует палитру на основе HSL-цвета.
        /// </summary>
        public static IReadOnlyList<ColorToken> Generate(HslColor baseColor, ColorHarmonyMode mode, int count = 5)
        {
            if (count <= 0) return [];

            count = Math.Min(count, maxHarmonyCount);

            return mode switch
            {
                ColorHarmonyMode.Monochrome => GenerateMonochrome(baseColor, count),
                ColorHarmonyMode.Analogous => GenerateByOffsets(baseColor, count, [-30, -15, 0, 15, 30]),
                ColorHarmonyMode.Complementary => GenerateByOffsets(baseColor, count, [0, 180]),
                ColorHarmonyMode.Triadic => GenerateByOffsets(baseColor, count, [0, 120, 240]),
                ColorHarmonyMode.Tetradic => GenerateByOffsets(baseColor, count, [0, 90, 180, 270]),
                _ => GenerateMonochrome(baseColor, count)
            };
        }

        private static IReadOnlyList<ColorToken> GenerateMonochrome(HslColor baseColor, int count)
        {
            if (count == 1) return [CreateToken(baseColor)];

            List<ColorToken> result = new (count);

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / (count - 1);
                double delta = (t - 0.5) * 0.72;

                double l = Math.Clamp(baseColor.L + delta, 0.08, 0.92);
                double s = Math.Clamp(baseColor.S * (1.0 - (Math.Abs(delta) / 0.36 * 0.25)), 0.0, 1.0);

                result.Add(CreateToken(baseColor with { S = s, L = l }));
            }

            return result;
        }

        private static IReadOnlyList<ColorToken> GenerateByOffsets(HslColor baseColor, int count, double[] offsets)
        {
            if (count <= offsets.Length)
            {
                int start = Math.Max(0, (offsets.Length - count) / 2);

                offsets = offsets.Skip(start).Take(count).ToArray();
            }

            List<ColorToken> result = new (count);

            for (int i = 0; i < count; i++)
            {
                double offset = offsets[i % offsets.Length];
                int cycle = i / offsets.Length;

                double lightDelta = cycle switch
                {
                    0 => 0.0,
                    1 => 0.10,
                    2 => -0.08,
                    3 => 0.16,
                    _ => (cycle % 2 == 1 ? 0.08 : -0.06) * cycle
                };

                double h = (baseColor.H + offset + 360.0) % 360.0;
                double l = Math.Clamp(baseColor.L + lightDelta, 0.08, 0.92);
                double s = Math.Clamp(baseColor.S * (cycle == 0 ? 1.0 : 0.92), 0.0, 1.0);

                result.Add(CreateToken(baseColor with { H = h, S = s, L = l }));
            }

            return result;
        }

        private static ColorToken CreateToken(HslColor hsl)
        {
            return new ColorToken
            {
                Hex = hsl.ToHex(),
                Source = ColorNameSource.Hex
            };
        }
    }
}
