using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfColors;

namespace ThemeForge.Abstractions.Records.UseOfTheme
{
    /// <summary>
    /// Простая цветовая тема на основе базового цвета и гармонии.
    /// </summary>
    /// <param name="Harmony">Режим гармонии.</param>
    /// <param name="BaseColor">Базовый цвет.</param>
    public sealed record SolidTheme(ColorHarmonyMode Harmony, ColorToken BaseColor)
    {
        /// <summary>
        /// Итоговая палитра. Может включать базовый цвет и производные цвета.
        /// Если пустая, палитра может быть вычислена сервисом гармонии.
        /// </summary>
        public IReadOnlyList<ColorToken> Palette { get; init; } = new List<ColorToken>();
    }

}
