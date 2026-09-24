using Abstractions.Enums;
using Abstractions.Helpers;
using Abstractions.Knowledge;
using Abstractions.Records.UseOfColors;
using Abstractions.Records.UseOfEffects;
using Abstractions.Records.UseOfGeometry;
using Abstractions.Records.UseOfTheme;
using Abstractions.Records.UseOfTypograhy;
using Core.Helpers;
using Maui.Helpers;
using GradientStop = Abstractions.Records.UseOfColors.Gradients.GradientStop;

namespace Maui.Services
{
    /// <summary>
    /// Строит MAUI-ресурсы из определения темы.
    /// </summary>
    public sealed class ThemeResourceBuilder
    {
        private static readonly string[] TypographyRoles =
        [
            "Body",
            "Title",
            "Caption"
        ];

        /// <summary>
        /// Создает словарь ресурсов для указанной темы.
        /// </summary>
        /// <param name="theme">Определение темы.</param>
        public ResourceDictionary Build(ThemeDefinition theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            ResourceDictionary resources = new ();

            AddSemanticResources(resources, theme);
            AddGlobalTypography(resources, theme.GlobalTypography);
            AddGlobalGeometry(resources, theme.GlobalGeometry);
            AddGlobalEffect(resources, theme.GlobalEffect);
            AddComponentResources(resources, theme.Components);

            return resources;
        }

        private static void AddSemanticResources(ResourceDictionary resources, ThemeDefinition theme)
        {
            string primaryHex;
            string secondaryHex;
            string tertiaryHex;
            string? quaternaryHex = null;

            Brush backgroundBrush;
            Brush primaryBrush;

            if (theme.Solid is not null)
            {
                primaryHex = theme.Solid.BaseColor.Hex;

                IReadOnlyList<ColorToken> palette = theme.Solid.Palette;

                if (palette.Count == 0)
                {
                    palette = ColorHarmonyGenerator.Generate(primaryHex, theme.Solid.Harmony, 5);
                }

                secondaryHex = palette.Count > 1 ? palette[1].Hex : ColorConversion.Lighten(primaryHex, 0.12);
                tertiaryHex = palette.Count > 2 ? palette[2].Hex : ColorConversion.Darken(primaryHex, 0.12);
                backgroundBrush = ColorConversion.ToSolidBrush(ColorConversion.Mix("#FFFFFF", primaryHex, 0.04));
                primaryBrush = ColorConversion.ToSolidBrush(primaryHex);
            }
            else if (theme.Gradient is not null && theme.Gradient.Stops.Count > 0)
            {
                IReadOnlyList<GradientStop> stops = theme.Gradient.Stops;

                primaryHex = stops.Count > 0 ? stops[0].Color.Hex : "#000000";
                secondaryHex = stops.Count > 1 ? stops[1].Color.Hex : ColorConversion.Lighten(primaryHex, 0.15);
                tertiaryHex = stops.Count > 2 ? stops[2].Color.Hex : ColorConversion.Darken(primaryHex, 0.15);
                quaternaryHex = stops.Count > 3 ? stops[3].Color.Hex : null;

                backgroundBrush = ColorConversion.ToBrush(theme.Gradient);
                primaryBrush = ColorConversion.ToBrush(theme.Gradient);
            }
            else
            {
                primaryHex = "#1976D2";
                secondaryHex = "#42A5F5";
                tertiaryHex = "#0D47A1";

                backgroundBrush = ColorConversion.ToSolidBrush("#FFFFFF");
                primaryBrush = ColorConversion.ToSolidBrush(primaryHex);
            }

            string onPrimaryHex = ColorConversion.GetContrastHex(primaryHex);
            string onSecondaryHex = ColorConversion.GetContrastHex(secondaryHex);
            string onTertiaryHex = ColorConversion.GetContrastHex(tertiaryHex);

            string surfaceHex = "#FFFFFF";
            string onSurfaceHex = "#1F1F1F";
            string backgroundHex = ColorConversion.Mix(surfaceHex, primaryHex, 0.03);

            string textPrimaryHex = "#1F1F1F";
            string textSecondaryHex = "#5F6368";
            string textDisabledHex = "#9AA0A6";
            string outlineHex = "#DADCE0";
            string outlineVariantHex = "#E8EAED";
            string disabledHex = "#E0E0E0";

            string errorHex = "#D32F2F";
            string successHex = "#2E7D32";
            string warningHex = "#F57C00";
            string infoHex = "#1976D2";

            AddColorAndBrush(resources, KnownColorRoles.Primary, primaryHex);
            AddColor(resources, KnownColorRoles.OnPrimary, onPrimaryHex);

            AddColorAndBrush(resources, KnownColorRoles.Secondary, secondaryHex);
            AddColor(resources, KnownColorRoles.OnSecondary, onSecondaryHex);

            AddColorAndBrush(resources, KnownColorRoles.Tertiary, tertiaryHex);
            AddColor(resources, KnownColorRoles.OnTertiary, onTertiaryHex);

            if (quaternaryHex is not null) AddColorAndBrush(resources, "Quaternary", quaternaryHex);

            AddColorAndBrush(resources, KnownColorRoles.Background, backgroundHex);
            AddColorAndBrush(resources, KnownColorRoles.Surface, surfaceHex);
            AddColor(resources, KnownColorRoles.OnSurface, onSurfaceHex);

            AddColorAndBrush(resources, KnownColorRoles.Outline, outlineHex);
            AddColorAndBrush(resources, KnownColorRoles.OutlineVariant, outlineVariantHex);
            AddColorAndBrush(resources, KnownColorRoles.Disabled, disabledHex);

            AddColor(resources, KnownColorRoles.TextPrimary, textPrimaryHex);
            AddColor(resources, KnownColorRoles.TextSecondary, textSecondaryHex);
            AddColor(resources, KnownColorRoles.TextDisabled, textDisabledHex);

            AddColor(resources, KnownColorRoles.IconPrimary, textPrimaryHex);
            AddColor(resources, KnownColorRoles.IconSecondary, textSecondaryHex);
            AddColor(resources, KnownColorRoles.IconDisabled, textDisabledHex);

            AddColorAndBrush(resources, KnownColorRoles.Error, errorHex);
            AddColorAndBrush(resources, KnownColorRoles.Success, successHex);
            AddColorAndBrush(resources, KnownColorRoles.Warning, warningHex);
            AddColorAndBrush(resources, KnownColorRoles.Info, infoHex);

            resources[ThemeResourceKeys.Brush("Background")] = backgroundBrush;
            resources[ThemeResourceKeys.Brush("Surface")] = ColorConversion.ToSolidBrush(surfaceHex);
            resources[ThemeResourceKeys.Brush("Primary")] = primaryBrush;
        }

        private static void AddGlobalTypography(ResourceDictionary resources, TypographySettings? typography)
        {
            if (typography is null) return;

            foreach (var role in TypographyRoles)
            {
                double scale = role switch
                {
                    "Title" => 1.25,
                    "Caption" => 0.85,
                    _ => 1.0
                };

                AddTypography(resources, typography, role, scale);
            }
        }

        private static void AddTypography(ResourceDictionary resources, TypographySettings typography, string role, double scale)
        {
            SetIfNotNull(resources, ThemeResourceKeys.FontFamily(role), typography.FontFamily);

            if (typography.FontSize.HasValue)
            {
                resources[ThemeResourceKeys.FontSize(role)] = typography.FontSize.Value * scale;
            }

            SetIfNotNull(resources, ThemeResourceKeys.FontBold(role), typography.IsBold);
            SetIfNotNull(resources, ThemeResourceKeys.FontItalic(role), typography.IsItalic);
            SetIfNotNull(resources, ThemeResourceKeys.FontStrikethrough(role), typography.HasStrikethrough);
            SetIfNotNull(resources, ThemeResourceKeys.FontUnderline(role), typography.HasUnderline);

            if (typography.StrikethroughStyle.HasValue)
            {
                resources[ThemeResourceKeys.FontStrikethroughStyle(role)] = typography.StrikethroughStyle.Value.ToString();
            }

            if (typography.UnderlineStyle.HasValue)
            {
                resources[ThemeResourceKeys.FontUnderlineStyle(role)] = typography.UnderlineStyle.Value.ToString();
            }
        }

        private static void AddGlobalGeometry(ResourceDictionary resources, GeometrySettings? geometry)
        {
            if (geometry is null) return;

            SetDouble(resources, ThemeResourceKeys.Geometry("CornerRadius"), geometry.CornerRadius);
            SetDouble(resources, ThemeResourceKeys.Geometry("BorderThickness"), geometry.BorderThickness);
            SetDouble(resources, ThemeResourceKeys.Geometry("Padding"), geometry.Padding);
            SetDouble(resources, ThemeResourceKeys.Geometry("Margin"), geometry.Margin);
            SetBool(resources, ThemeResourceKeys.Geometry("HasShadow"), geometry.HasShadow);
            SetDouble(resources, ThemeResourceKeys.Geometry("ShadowRadius"), geometry.ShadowRadius);
            SetDouble(resources, ThemeResourceKeys.Geometry("ShadowOpacity"), geometry.ShadowOpacity);
            SetDouble(resources, ThemeResourceKeys.Geometry("Elevation"), geometry.Elevation);
        }

        private static void AddGlobalEffect(ResourceDictionary resources, EffectSettings? effect)
        {
            if (effect is null) return;

            AddEffect(resources, effect, "Theme.Effect.");
        }

        private static void AddComponentResources(ResourceDictionary resources, IReadOnlyDictionary<string, ComponentTheme> components)
        {
            foreach (KeyValuePair<string, ComponentTheme> pair in components)
            {
                if (!ThemeComponentKey.TryParse(pair.Key, out string controlType, out ComponentState state)) continue;

                ComponentTheme component = pair.Value;
                string prefix = ThemeResourceKeys.ControlPrefix(controlType, state);

                AddComponentResource(resources, prefix, component);
            }
        }

        private static void AddComponentResource(ResourceDictionary resources, string prefix, ComponentTheme component)
        {
            if (component.BackgroundGradient is not null)
            {
                resources[prefix + "Background"] = ColorConversion.ToBrush(component.BackgroundGradient);
            }
            else if (component.Background is not null)
            {
                Color backgroundColor = ColorConversion.ToColor(component.Background.Hex);
                resources[prefix + "Background"] = new SolidColorBrush(backgroundColor);
                resources[prefix + "BackgroundColor"] = backgroundColor;
            }

            if (component.Foreground is not null) resources[prefix + "Foreground"] = ColorConversion.ToColor(component.Foreground.Hex);

            if (component.Border is not null) resources[prefix + "Border"] = ColorConversion.ToColor(component.Border.Hex);

            if (component.NamedColors is not null)
            {
                foreach (KeyValuePair<string, ColorToken> namedColor in component.NamedColors)
                {
                    Color color = ColorConversion.ToColor(namedColor.Value.Hex);
                    resources[$"{prefix}Color.{namedColor.Key}"] = color;
                    resources[$"{prefix}Brush.{namedColor.Key}"] = new SolidColorBrush(color);
                }
            }

            if (component.Typography is not null) AddComponentTypography(resources, prefix, component.Typography);

            if (component.Geometry is not null) AddComponentGeometry(resources, prefix, component.Geometry);

            if (component.Effects is not null) AddEffect(resources, component.Effects, prefix + "Effect.");
        }

        private static void AddComponentTypography(ResourceDictionary resources, string prefix, TypographySettings typography)
        {
            SetIfNotNull(resources, prefix + "FontFamily", typography.FontFamily);
            SetDouble(resources, prefix + "FontSize", typography.FontSize);
            SetBool(resources, prefix + "FontBold", typography.IsBold);
            SetBool(resources, prefix + "FontItalic", typography.IsItalic);
            SetBool(resources, prefix + "HasStrikethrough", typography.HasStrikethrough);
            SetBool(resources, prefix + "HasUnderline", typography.HasUnderline);

            if (typography.StrikethroughStyle.HasValue)
            {
                resources[prefix + "StrikethroughStyle"] = typography.StrikethroughStyle.Value.ToString();
            }

            if (typography.UnderlineStyle.HasValue)
            {
                resources[prefix + "UnderlineStyle"] = typography.UnderlineStyle.Value.ToString();
            }
        }

        private static void AddComponentGeometry(ResourceDictionary resources, string prefix, GeometrySettings geometry)
        {
            SetDouble(resources, prefix + "Width", geometry.Width);
            SetDouble(resources, prefix + "Height", geometry.Height);
            SetDouble(resources, prefix + "CornerRadius", geometry.CornerRadius);
            SetDouble(resources, prefix + "BorderThickness", geometry.BorderThickness);
            SetDouble(resources, prefix + "Padding", geometry.Padding);
            SetDouble(resources, prefix + "Margin", geometry.Margin);
            SetDouble(resources, prefix + "ThumbDiameter", geometry.ThumbDiameter);
            SetDouble(resources, prefix + "TrackThickness", geometry.TrackThickness);
            SetDouble(resources, prefix + "IndicatorSize", geometry.IndicatorSize);
            SetBool(resources, prefix + "HasShadow", geometry.HasShadow);
            SetDouble(resources, prefix + "ShadowRadius", geometry.ShadowRadius);
            SetDouble(resources, prefix + "ShadowOpacity", geometry.ShadowOpacity);
            SetDouble(resources, prefix + "Elevation", geometry.Elevation);
            SetDouble(resources, prefix + "Rotation", geometry.Rotation);
            SetDouble(resources, prefix + "Scale", geometry.Scale);
        }

        private static void AddEffect(ResourceDictionary resources, EffectSettings effect, string prefix)
        {
            resources[prefix + "Enabled"] = effect.IsEnabled;
            resources[prefix + "Kind"] = effect.Kind.ToString();
            resources[prefix + "Intensity"] = effect.Intensity;
            resources[prefix + "Speed"] = effect.Speed;

            foreach (KeyValuePair<string,string> parameter in effect.Parameters)
            {
                resources[prefix + "Param." + parameter.Key] = parameter.Value;
            }
        }

        private static void AddColor(ResourceDictionary resources, string role, string hex) => resources[ThemeResourceKeys.Color(role)] = ColorConversion.ToColor(hex);

        private static void AddColorAndBrush(ResourceDictionary resources, string role, string hex)
        {
            Color color = ColorConversion.ToColor(hex);
            resources[ThemeResourceKeys.Color(role)] = color;
            resources[ThemeResourceKeys.Brush(role)] = new SolidColorBrush(color);
        }

        private static void SetIfNotNull(ResourceDictionary resources, string key, object? value)
        {
            if (value is not null)
            {
                resources[key] = value;
            }
        }

        private static void SetDouble(ResourceDictionary resources, string key, double? value)
        {
            if (value.HasValue)
            {
                resources[key] = value.Value;
            }
        }

        private static void SetBool(ResourceDictionary resources, string key, bool? value)
        {
            if (value.HasValue)
            {
                resources[key] = value.Value;
            }
        }
    }
}
