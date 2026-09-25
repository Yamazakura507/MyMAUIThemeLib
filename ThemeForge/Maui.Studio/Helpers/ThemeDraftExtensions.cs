using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Helpers;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Abstractions.Records.UseOfGeometry;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Abstractions.Records.UseOfTypograhy;

namespace ThemeForge.Maui.Studio.Helpers
{
    /// <summary>
    /// Расширения для безопасного обновления immutable-моделей темы.
    /// </summary>
    public static class ThemeDraftExtensions
    {
        /// <summary>
        /// Возвращает копию темы с обновленным компонентом.
        /// </summary>
        public static ThemeDefinition WithComponent(this ThemeDefinition theme, string controlType, ComponentState state, ComponentTheme component)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(controlType);

            string key = ThemeComponentKey.Create(controlType, state);
            Dictionary<string, ComponentTheme> components = new (theme.Components);
            components[key] = component;

            return theme with
            {
                Components = components,
                UpdatedUtc = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Обновляет типографику конкретного компонента, сохраняя остальные поля.
        /// </summary>
        public static ThemeDefinition WithComponentTypography(this ThemeDefinition theme, string controlType, ComponentState state, TypographySettings typography)
        {
            ComponentTheme existing = theme.GetComponentTheme(controlType, state);
            ComponentTheme updated = existing with
            {
                ControlType = controlType,
                State = state,
                Typography = typography
            };

            return theme.WithComponent(controlType, state, updated);
        }

        /// <summary>
        /// Обновляет геометрию конкретного компонента, сохраняя остальные поля.
        /// </summary>
        public static ThemeDefinition WithComponentGeometry(this ThemeDefinition theme, string controlType, ComponentState state, GeometrySettings geometry)
        {
            ComponentTheme existing = theme.GetComponentTheme(controlType, state);
            ComponentTheme updated = existing with
            {
                ControlType = controlType,
                State = state,
                Geometry = geometry
            };

            return theme.WithComponent(controlType, state, updated);
        }

        /// <summary>
        /// Получает существующую тему компонента или создает пустую.
        /// </summary>
        public static ComponentTheme GetComponentTheme(this ThemeDefinition theme, string controlType, ComponentState state)
        {
            string key = ThemeComponentKey.Create(controlType, state);

            return theme.Components.TryGetValue(key, out ComponentTheme? component) ? component : new ComponentTheme(controlType) { State = state };
        }

        /// <summary>
        /// Обновляет эффект конкретного компонента, сохраняя остальные поля.
        /// </summary>
        public static ThemeDefinition WithComponentEffects(this ThemeDefinition theme, string controlType, ComponentState state, EffectSettings effects)
        {
            ComponentTheme existing = theme.GetComponentTheme(controlType, state);
            ComponentTheme updated = existing with
            {
                ControlType = controlType,
                State = state,
                Effects = effects
            };

            return theme.WithComponent(controlType, state, updated);
        }
    }
}
