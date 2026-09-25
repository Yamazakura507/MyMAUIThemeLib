using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Helpers;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Knowledge;
using ThemeForge.Abstractions.Records;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Abstractions.Records.UseOfGeometry;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Abstractions.Records.UseOfTypograhy;
using ThemeForge.Core.Helpers;
using ThemeForge.Maui.Helpers;
using GradientStop = ThemeForge.Abstractions.Records.UseOfColors.Gradients.GradientStop;

namespace ThemeForge.Maui.Services
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

        private readonly IControlThemeCatalog? catalog;

        /// <summary>
        /// Создает строитель ресурсов темы.
        /// </summary>
        public ThemeResourceBuilder(IControlThemeCatalog? catalog = null)
        {
            this.catalog = catalog;
        }

        /// <summary>
        /// Создает словарь ресурсов для указанной темы.
        /// </summary>
        public ResourceDictionary Build(ThemeDefinition theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            ResourceDictionary resources = new ();

            AddSemanticResources(resources, theme);
            AddGlobalTypography(resources, theme.GlobalTypography);
            AddGlobalGeometry(resources, theme.GlobalGeometry);
            AddGlobalEffect(resources, theme.GlobalEffect);
            EnsureComponentDefaults(resources, theme);
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

            if (quaternaryHex is not null)
            {
                AddColorAndBrush(resources, "Quaternary", quaternaryHex);
            }

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

            foreach (string role in TypographyRoles)
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

        private void EnsureComponentDefaults(ResourceDictionary resources, ThemeDefinition theme)
        {
            if (catalog is null) return;

            foreach (ControlThemeDescriptor descriptor in catalog.GetDescriptors())
            {
                foreach (ComponentState state in descriptor.SupportedStates)
                {
                    string key = ThemeComponentKey.Create(descriptor.ControlType, state);

                    theme.Components.TryGetValue(key, out var explicitComponent);

                    AddComponentDefaults(resources, theme, descriptor.ControlType, state, explicitComponent);
                }
            }
        }

        private static void AddComponentDefaults(ResourceDictionary resources, ThemeDefinition theme, string controlType, ComponentState state, ComponentTheme? explicitComponent)
        {
            string prefix = ThemeResourceKeys.ControlPrefix(controlType, state);

            string primaryHex = GetPrimaryHex(theme);
            string onPrimaryHex = ColorConversion.GetContrastHex(primaryHex);

            const string surfaceHex = "#FFFFFF";
            const string textPrimaryHex = "#1F1F1F";
            const string textSecondaryHex = "#5F6368";
            const string textDisabledHex = "#9AA0A6";
            const string outlineHex = "#DADCE0";
            const string outlineVariantHex = "#E8EAED";
            const string disabledHex = "#E0E0E0";
            const string transparentHex = "#00FFFFFF";

            bool isDisabled = state.HasFlag(ComponentState.Disabled);
            bool isSelected = state.HasFlag(ComponentState.Selected);
            bool isOn = state.HasFlag(ComponentState.On);
            bool isChecked = state.HasFlag(ComponentState.Checked);

            string backgroundHex;
            string foregroundHex;
            string borderHex;
            double cornerRadius = 8;
            double borderThickness = 1;
            double padding = 12;
            double thumbDiameter = 16;
            double trackThickness = 4;
            double indicatorSize = 18;

            Dictionary<string, string> named = new (StringComparer.OrdinalIgnoreCase)
            {
                [KnownNamedColorRoles.Text] = textPrimaryHex,
                [KnownNamedColorRoles.Placeholder] = textSecondaryHex,
                [KnownNamedColorRoles.Icon] = textPrimaryHex,
                [KnownNamedColorRoles.Ripple] = primaryHex,
                [KnownNamedColorRoles.Background] = surfaceHex,
                [KnownNamedColorRoles.Border] = outlineHex,
                [KnownNamedColorRoles.Track] = primaryHex,
                [KnownNamedColorRoles.MaximumTrack] = outlineHex,
                [KnownNamedColorRoles.Thumb] = primaryHex,
                [KnownNamedColorRoles.Indicator] = primaryHex,
                [KnownNamedColorRoles.Progress] = primaryHex
            };

            switch (controlType)
            {
                case KnownControlTypes.Button:
                    backgroundHex = isDisabled ? disabledHex : primaryHex;
                    foregroundHex = isDisabled ? textDisabledHex : onPrimaryHex;
                    borderHex = transparentHex;
                    cornerRadius = 8;
                    borderThickness = 0;
                    padding = 12;
                    named[KnownNamedColorRoles.Icon] = foregroundHex;
                    break;
                case KnownControlTypes.Entry:
                case KnownControlTypes.Editor:
                case KnownControlTypes.SearchBar:
                case KnownControlTypes.Picker:
                case KnownControlTypes.DatePicker:
                case KnownControlTypes.TimePicker:
                    backgroundHex = isDisabled ? disabledHex : surfaceHex;
                    foregroundHex = isDisabled ? textDisabledHex : textPrimaryHex;
                    borderHex = outlineHex;
                    cornerRadius = 6;
                    borderThickness = 1;
                    padding = 10;
                    named[KnownNamedColorRoles.Placeholder] = textSecondaryHex;
                    named[KnownNamedColorRoles.Icon] = textSecondaryHex;
                    break;
                case KnownControlTypes.Switch:
                    backgroundHex = isDisabled
                        ? disabledHex
                        : isOn
                            ? primaryHex
                            : outlineHex;
                    foregroundHex = isDisabled ? textDisabledHex : onPrimaryHex;
                    borderHex = transparentHex;
                    indicatorSize = 20;
                    named[KnownNamedColorRoles.Thumb] = isDisabled ? textDisabledHex : "#FFFFFF";
                    named[KnownNamedColorRoles.Track] = backgroundHex;
                    named[KnownNamedColorRoles.Indicator] = backgroundHex;
                    break;
                case KnownControlTypes.CheckBox:
                case KnownControlTypes.RadioButton:
                    backgroundHex = surfaceHex;
                    foregroundHex = isDisabled ? textDisabledHex : primaryHex;
                    borderHex = outlineHex;
                    indicatorSize = 18;
                    named[KnownNamedColorRoles.Indicator] = foregroundHex;
                    named[KnownNamedColorRoles.Icon] = foregroundHex;
                    break;
                case KnownControlTypes.Slider:
                    backgroundHex = transparentHex;
                    foregroundHex = isDisabled ? textDisabledHex : primaryHex;
                    borderHex = transparentHex;
                    trackThickness = 4;
                    thumbDiameter = 16;
                    named[KnownNamedColorRoles.Track] = isDisabled ? disabledHex : primaryHex;
                    named[KnownNamedColorRoles.MaximumTrack] = outlineHex;
                    named[KnownNamedColorRoles.Thumb] = isDisabled ? disabledHex : primaryHex;
                    break;
                case KnownControlTypes.ProgressBar:
                    backgroundHex = outlineVariantHex;
                    foregroundHex = isDisabled ? disabledHex : primaryHex;
                    borderHex = transparentHex;
                    trackThickness = 6;
                    named[KnownNamedColorRoles.Progress] = foregroundHex;
                    named[KnownNamedColorRoles.Track] = backgroundHex;
                    break;
                case KnownControlTypes.Tab:
                    backgroundHex = isSelected ? primaryHex : surfaceHex;
                    foregroundHex = isSelected ? onPrimaryHex : textPrimaryHex;
                    borderHex = outlineHex;
                    cornerRadius = 6;
                    padding = 10;
                    break;
                case KnownControlTypes.Card:
                    backgroundHex = surfaceHex;
                    foregroundHex = textPrimaryHex;
                    borderHex = outlineHex;
                    cornerRadius = 12;
                    borderThickness = 1;
                    padding = 12;
                    break;
                default:
                    backgroundHex = isDisabled ? disabledHex : surfaceHex;
                    foregroundHex = isDisabled ? textDisabledHex : textPrimaryHex;
                    borderHex = outlineHex;
                    break;
            }

            if (isDisabled) foregroundHex = textDisabledHex;

            SetResource(resources, prefix + "Background", new SolidColorBrush(ColorConversion.ToColor(backgroundHex)));
            SetResource(resources, prefix + "BackgroundColor", ColorConversion.ToColor(backgroundHex));
            SetResource(resources, prefix + "Foreground", ColorConversion.ToColor(foregroundHex));
            SetResource(resources, prefix + "Border", ColorConversion.ToColor(borderHex));

            SetResource(resources, prefix + "CornerRadius", cornerRadius);
            SetResource(resources, prefix + "BorderThickness", borderThickness);
            SetResource(resources, prefix + "Padding", padding);
            SetResource(resources, prefix + "ThumbDiameter", thumbDiameter);
            SetResource(resources, prefix + "TrackThickness", trackThickness);
            SetResource(resources, prefix + "IndicatorSize", indicatorSize);

            foreach (KeyValuePair<string,string> pair in named)
            {
                Color color = ColorConversion.ToColor(pair.Value);
                SetResource(resources, $"{prefix}Color.{pair.Key}", color);
                SetResource(resources, $"{prefix}Brush.{pair.Key}", new SolidColorBrush(color));
            }
            if (explicitComponent?.Geometry is not null) AddComponentGeometry(resources, prefix, explicitComponent.Geometry);
        }

        private static void AddComponentResources(ResourceDictionary resources, IReadOnlyDictionary<string, ComponentTheme> components)
        {
            foreach (var pair in components)
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
                Brush brush = ColorConversion.ToBrush(component.BackgroundGradient);
                resources[prefix + "Background"] = brush;

                if (component.BackgroundGradient.Stops.Count > 0)
                {
                    resources[prefix + "BackgroundColor"] = ColorConversion.ToColor(component.BackgroundGradient.Stops[0].Color.Hex);
                }
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
                foreach (KeyValuePair<string,ColorToken> namedColor in component.NamedColors)
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

        private static void AddColor(ResourceDictionary resources, string role, string hex)
        {
            resources[ThemeResourceKeys.Color(role)] = ColorConversion.ToColor(hex);
        }

        private static void AddColorAndBrush(ResourceDictionary resources, string role, string hex)
        {
            Color color = ColorConversion.ToColor(hex);
            resources[ThemeResourceKeys.Color(role)] = color;
            resources[ThemeResourceKeys.Brush(role)] = new SolidColorBrush(color);
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

        private static void SetResource(ResourceDictionary resources, string key, object value)
        {
            resources[key] = value;
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
