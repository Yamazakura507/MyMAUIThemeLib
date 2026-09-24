using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfColors.Gradients;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Abstractions.Interfaces
{
    /// <summary>
    /// Фабрика тем. Создает корректные ThemeDefinition из пользовательского ввода.
    /// </summary>
    public interface IThemeFactory
    {
        /// <summary>
        /// Создает простую цветовую тему.
        /// </summary>
        ValueTask<ThemeDefinition> CreateSolidAsync(ColorHarmonyMode harmony, string baseHex, string? name = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Создает градиентную тему.
        /// </summary>
        ValueTask<ThemeDefinition> CreateGradientAsync(
            GradientType type,
            IReadOnlyList<string> hexes,
            GradientGeometry? geometry = null,
            EffectSettings? effect = null,
            string? name = null,
            CancellationToken cancellationToken = default);
    }
}
