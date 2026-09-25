using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfColors.Gradients;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Maui.Helpers;
using GradientStop = ThemeForge.Abstractions.Records.UseOfColors.Gradients.GradientStop;

namespace ThemeForge.Maui.Controls.Helpers
{
    /// <summary>
    /// Создает отображаемый brush и эффект для карточек/preview на основе темы.
    /// </summary>
    public static class ThemeDisplayBrushFactory
    {
        /// <summary>
        /// Создает brush для визуального представления темы.
        /// </summary>
        public static Brush CreateDisplayBrush(ThemeDefinition? theme, ThemePresentationMode mode)
        {
            if (theme is null) return new SolidColorBrush(Colors.Gray);

            return mode switch
            {
                ThemePresentationMode.Solid => ColorConversion.ToSolidBrush(GetPrimaryHex(theme)),
                ThemePresentationMode.Automatic or ThemePresentationMode.Animated when theme.Gradient is not null => theme.Gradient.ToBrush(),
                ThemePresentationMode.LinearGradient => CreateSyntheticGradient(theme, GradientType.Linear).ToBrush(),
                ThemePresentationMode.RadialGradient => CreateSyntheticGradient(theme, GradientType.Radial).ToBrush(),
                _ when theme.Gradient is not null => theme.Gradient.ToBrush(),
                _ => ColorConversion.ToSolidBrush(GetPrimaryHex(theme))
            };
        }

        /// <summary>
        /// Разрешает эффект для отображения темы.
        /// </summary>
        public static EffectSettings? ResolveEffect(ThemeDefinition? theme)
        {
            if (theme is null) return null;

            if (theme.Gradient?.BackgroundEffect is { IsEnabled: true } gradientEffect)
            {
                return gradientEffect;
            }

            return theme.GlobalEffect;
        }

        public static T? GetService<T>(this ContentView view) where T : class => 
            view.Handler?.MauiContext?.Services.GetService<T>() ?? Application.Current?.Handler?.MauiContext?.Services.GetService<T>();

        private static GradientTheme CreateSyntheticGradient(ThemeDefinition theme, GradientType type)
        {
            IReadOnlyList<string> colors = GetDisplayColors(theme);

            if (colors.Count == 0) colors = ["#1976D2"];

            List<GradientStop> stops = new (colors.Count);

            for (int i = 0; i < colors.Count; i++)
            {
                double offset = colors.Count == 1 ? 0 : (double)i / (colors.Count - 1);

                stops.Add(new GradientStop(ColorToken.FromHex(colors[i]), offset));
            }

            GradientGeometry? geometry = type switch
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

            return new GradientTheme(type, stops) { Geometry = geometry };
        }

        private static IReadOnlyList<string> GetDisplayColors(ThemeDefinition theme)
        {
            if (theme.Gradient is not null && theme.Gradient.Stops.Count > 0)
            {
                return theme.Gradient.Stops.Select(s => s.Color.Hex).ToList();
            }

            if (theme.Solid is not null)
            {
                if (theme.Solid.Palette.Count > 0)
                {
                    return theme.Solid.Palette.Select(p => p.Hex).ToList();
                }

                string baseHex = theme.Solid.BaseColor.Hex;

                return
                [
                    baseHex,
                    ColorConversion.Lighten(baseHex, 0.12),
                    ColorConversion.Darken(baseHex, 0.12)
                ];
            }

            return ["#1976D2"];
        }

        private static string GetPrimaryHex(ThemeDefinition theme)
        {
            if (theme.Solid is not null)
            {
                return theme.Solid.BaseColor.Hex;
            }

            if (theme.Gradient is not null && theme.Gradient.Stops.Count > 0)
            {
                return theme.Gradient.Stops[0].Color.Hex;
            }

            return "#1976D2";
        }
    }
}
