using Abstractions.Enums;
using Abstractions.Interfaces;
using Abstractions.Records.UseOfColors;
using Abstractions.Records.UseOfColors.Gradients;
using Abstractions.Records.UseOfEffects;
using Abstractions.Records.UseOfTheme;
using Core.Helpers;

namespace Core.Services.Theme
{
    /// <summary>
    /// Фабрика для создания тем из пользовательского ввода.
    /// </summary>
    public sealed class ThemeFactory : IThemeFactory
    {
        private readonly IColorHarmonyService harmonyService;
        private readonly IColorNamingService colorNamingService;
        private readonly IThemeNamingService themeNamingService;

        /// <summary>
        /// Создает фабрику тем.
        /// </summary>
        public ThemeFactory(IColorHarmonyService harmonyService, IColorNamingService colorNamingService, IThemeNamingService themeNamingService)
        {
            this.harmonyService = harmonyService;
            this.colorNamingService = colorNamingService;
            this.themeNamingService = themeNamingService;
        }

        /// <inheritdoc />
        public async ValueTask<ThemeDefinition> CreateSolidAsync(ColorHarmonyMode harmony, string baseHex, string? name = null, CancellationToken cancellationToken = default)
        {
            string normalized = ColorUtility.NormalizeHex(baseHex);
            ColorToken baseToken = await colorNamingService.GetNameAsync(normalized, cancellationToken);
            IReadOnlyList<ColorToken> palette = await harmonyService.GenerateAsync(baseToken, harmony, count: 5, cancellationToken: cancellationToken);

            ThemeDefinition theme = new (name ?? string.Empty, ThemeKind.Solid)
            {
                Solid = new SolidTheme(harmony, baseToken)
                {
                    Palette = palette
                }
            };

            if (string.IsNullOrWhiteSpace(theme.Name))
            {
                theme = theme with { Name = themeNamingService.BuildDisplayName(theme) };
            }

            return theme;
        }

        /// <inheritdoc />
        public async ValueTask<ThemeDefinition> CreateGradientAsync(
            GradientType type,
            IReadOnlyList<string> hexes,
            GradientGeometry? geometry = null,
            EffectSettings? effect = null,
            string? name = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(hexes);

            if (hexes.Count < 2 || hexes.Count > 4) throw new ArgumentException("Градиентная тема должна содержать от 2 до 4 цветов.", nameof(hexes));

            List<string> normalized = hexes.Select(ColorUtility.NormalizeHex).ToList();

            IReadOnlyDictionary<string,ColorToken> named = await colorNamingService.GetNamesAsync(normalized, cancellationToken);

            List<GradientStop> stops = new (normalized.Count);

            for (int i = 0; i < normalized.Count; i++)
            {
                string hex = normalized[i];

                ColorToken token = named.TryGetValue(hex, out ColorToken? resolved) ? resolved : ColorToken.FromHex(hex);
                double offset = normalized.Count == 1 ? 0 : (double)i / (normalized.Count - 1);

                stops.Add(new GradientStop(token, offset));
            }

            geometry ??= CreateDefaultGeometry(type);

            ThemeKind kind = type == GradientType.Linear ? ThemeKind.LinearGradient : ThemeKind.RadialGradient;

            ThemeDefinition theme = new (name ?? string.Empty, kind)
            {
                Gradient = new GradientTheme(type, stops)
                {
                    Geometry = geometry,
                    BackgroundEffect = effect
                }
            };

            if (string.IsNullOrWhiteSpace(theme.Name))
            {
                theme = theme with { Name = themeNamingService.BuildDisplayName(theme) };
            }

            return theme;
        }

        private static GradientGeometry CreateDefaultGeometry(GradientType type)
        {
            return type switch
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
        }
    }
}
