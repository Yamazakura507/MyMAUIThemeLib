using Abstractions.Enums;
using Abstractions.Interfaces;
using Abstractions.Records.UseOfColors;
using Abstractions.Records.UseOfTheme;
using Core.Interfaces;

namespace Core.Services.Name
{
    /// <summary>
    /// Формирует человекочитаемые имена тем по правилам Ready-themes.
    /// </summary>
    public sealed class ThemeNamingService : IThemeNamingService
    {
        private readonly IColorNameLookup? lookup;

        /// <summary>
        /// Создает сервис именования тем.
        /// </summary>
        /// <param name="lookup">Опциональный синхронный lookup имен цветов.</param>
        public ThemeNamingService(IColorNameLookup? lookup = null)
        {
            this.lookup = lookup;
        }

        /// <inheritdoc />
        public string BuildDisplayName(ThemeDefinition theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            if (theme.Solid is not null)
            {
                string harmony = GetHarmonyName(theme.Solid.Harmony);
                string colorName = ResolveColorName(theme.Solid.BaseColor);

                return $"{harmony} - {colorName}";
            }

            if (theme.Gradient is not null)
            {
                string gradientName = GetGradientName(theme.Gradient.Type);
                int count = theme.Gradient.Stops.Count;
                string names = string.Join(", ", theme.Gradient.Stops.Select(s => ResolveColorName(s.Color)));

                return $"{gradientName} - {count} colors: {names}";
            }

            return string.IsNullOrWhiteSpace(theme.Name) ? "Theme" : theme.Name;
        }

        /// <inheritdoc />
        public string BuildPresetTitle(ThemeDefinition theme) => BuildDisplayName(theme);

        /// <inheritdoc />
        public string BuildPresetDetail(ThemeDefinition theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            if (theme.Solid is not null)
            {
                if (theme.Solid.Palette.Count == 0)
                {
                    return ResolveColorName(theme.Solid.BaseColor);
                }

                return string.Join(" · ", theme.Solid.Palette.Take(6).Select(ResolveColorName));
            }

            if (theme.Gradient is not null)
            {
                string names = string.Join(", ", theme.Gradient.Stops.Select(s => ResolveColorName(s.Color)));

                string geometry = theme.Gradient.Type switch
                {
                    GradientType.Linear => $"angle {theme.Gradient.Geometry.AngleDegrees:F0}°",
                    GradientType.Radial => $"center ({theme.Gradient.Geometry.CenterPoint?.X:F2}, {theme.Gradient.Geometry.CenterPoint?.Y:F2}), radius {theme.Gradient.Geometry.Radius:F2}",
                    _ => string.Empty
                };

                string effect = theme.Gradient.BackgroundEffect is { IsEnabled: true } bgEffect ? $" · effect {bgEffect.Kind}" : string.Empty;

                return $"{names} · {geometry}{effect}";
            }

            return theme.Name;
        }

        private string ResolveColorName(ColorToken token)
        {
            if (!string.IsNullOrWhiteSpace(token.Name)) return token.Name;

            string? localName = lookup?.Lookup(token.Hex);

            return localName ?? token.Hex;
        }

        private static string GetHarmonyName(ColorHarmonyMode mode)
        {
            return mode switch
            {
                ColorHarmonyMode.Monochrome => "Monochrome",
                ColorHarmonyMode.Analogous => "Analogous",
                ColorHarmonyMode.Complementary => "Complementary",
                ColorHarmonyMode.Triadic => "Triadic",
                ColorHarmonyMode.Tetradic => "Quad",
                _ => mode.ToString()
            };
        }

        private static string GetGradientName(GradientType type)
        {
            return type switch
            {
                GradientType.Linear => "Linear Gradient",
                GradientType.Radial => "Radial Gradient",
                _ => type.ToString()
            };
        }
    }
}
