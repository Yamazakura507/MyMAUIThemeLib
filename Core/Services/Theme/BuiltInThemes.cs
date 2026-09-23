using Abstractions.Enums;
using Abstractions.Records.UseOfColors;
using Abstractions.Records.UseOfColors.Gradients;
using Abstractions.Records.UseOfTheme;
using Core.Helpers;
using Core.Services.Name;

namespace Core.Services.Theme
{
    /// <summary>
    /// Встроенные готовые темы.
    /// </summary>
    public static class BuiltInThemes
    {
        private static readonly Lazy<IReadOnlyList<ThemeDefinition>> DefaultThemes = new(CreateCore);

        /// <summary>
        /// Возвращает список встроенных тем.
        /// </summary>
        public static IReadOnlyList<ThemeDefinition> CreateDefaultThemes() => DefaultThemes.Value;

        /// <summary>
        /// Возвращает резервную тему.
        /// </summary>
        public static ThemeDefinition CreateFallbackTheme()
        {
            IReadOnlyList<ThemeDefinition> themes = DefaultThemes.Value;

            return themes.Count > 0 ? themes[0] : CreateEmptyTheme();
        }

        private static IReadOnlyList<ThemeDefinition> CreateCore()
        {
            ThemeNamingService naming = new (new LocalColorNameProvider());

            return
            [
                CreateSolid(naming, ColorHarmonyMode.Monochrome, "#2E7D32"),
                CreateSolid(naming, ColorHarmonyMode.Analogous, "#1976D2"),
                CreateSolid(naming, ColorHarmonyMode.Complementary, "#F57C00"),
                CreateSolid(naming, ColorHarmonyMode.Triadic, "#00796B"),
                CreateSolid(naming, ColorHarmonyMode.Tetradic, "#9C27B0"),
                CreateGradient(naming, GradientType.Linear, "#1976D2", "#7E57C2", "#EC407A"),
                CreateGradient(naming, GradientType.Radial, "#FFC107", "#F57C00", "#D32F2F", "#B71C1C")
            ];
        }

        private static ThemeDefinition CreateSolid(ThemeNamingService naming, ColorHarmonyMode harmony, string hex)
        {
            string? normalized = ColorUtility.NormalizeHex(hex);
            IReadOnlyList<ColorToken> palette = ColorHarmonyGenerator.Generate(normalized, harmony, 5);

            ThemeDefinition theme = new (string.Empty, ThemeKind.Solid)
            {
                Solid = new SolidTheme(harmony, new ColorToken { Hex = normalized })
                {
                    Palette = palette
                },
                IsBuiltIn = true
            };

            return theme with { Name = naming.BuildDisplayName(theme) };
        }

        private static ThemeDefinition CreateGradient(ThemeNamingService naming, GradientType type, params string[] hexes)
        {
            IReadOnlyList<GradientStop> stops = CreateStops(hexes);

            GradientGeometry geometry = type switch
            {
                GradientType.Linear => new GradientGeometry
                {
                    AngleDegrees = 90,
                    StartPoint = new NormalizedPoint(0, 0.5),
                    EndPoint = new NormalizedPoint(1, 0.5)
                },
                GradientType.Radial => new GradientGeometry
                {
                    CenterPoint = new NormalizedPoint(0.5, 0.5),
                    Radius = 0.75
                },
                _ => new GradientGeometry()
            };

            ThemeKind kind = type == GradientType.Linear ? ThemeKind.LinearGradient : ThemeKind.RadialGradient;

            ThemeDefinition theme = new (string.Empty, kind)
            {
                Gradient = new GradientTheme(type, stops)
                {
                    Geometry = geometry
                },
                IsBuiltIn = true
            };

            return theme with { Name = naming.BuildDisplayName(theme) };
        }

        private static IReadOnlyList<GradientStop> CreateStops(string[] hexes)
        {
            List<GradientStop> stops = new (hexes.Length);

            for (int i = 0; i < hexes.Length; i++)
            {
                double offset = hexes.Length == 1 ? 0 : (double)i / (hexes.Length - 1);

                stops.Add(new GradientStop(new ColorToken { Hex = ColorUtility.NormalizeHex(hexes[i]) }, offset));
            }

            return stops;
        }

        private static ThemeDefinition CreateEmptyTheme()
        {
            return new ThemeDefinition("Default", ThemeKind.Solid)
            {
                Solid = new SolidTheme(ColorHarmonyMode.Monochrome, ColorToken.FromHex("#000000")),
                IsBuiltIn = true
            };
        }
    }
}
