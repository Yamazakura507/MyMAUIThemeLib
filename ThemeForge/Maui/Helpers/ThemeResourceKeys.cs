using ThemeForge.Abstractions.Enums;

namespace ThemeForge.Maui.Helpers
{
    /// <summary>
    /// Централизованные ключи ресурсов MAUI для системы тем.
    /// </summary>
    public static class ThemeResourceKeys
    {
        /// <summary>
        /// Маркер словаря значений по умолчанию.
        /// </summary>
        public const string DefaultsDictionaryMarker = "ThemeForge.Defaults";

        /// <summary>
        /// Маркер словаря runtime-темы.
        /// </summary>
        public const string RuntimeDictionaryMarker = "ThemeForge.Runtime";

        /// <summary>
        /// Ключ цвета по семантической роли.
        /// </summary>
        /// <param name="role">Роль цвета, например Primary, Surface, TextPrimary.</param>
        public static string Color(string role) => $"Theme.Color.{role}";

        /// <summary>
        /// Ключ кисти по семантической роли.
        /// </summary>
        /// <param name="role">Роль кисти, например Primary, Background, Surface.</param>
        public static string Brush(string role) => $"Theme.Brush.{role}";

        /// <summary>
        /// Ключ семейства шрифта для роли текста.
        /// </summary>
        /// <param name="role">Роль текста, например Body, Title, Caption.</param>
        public static string FontFamily(string role) => $"Theme.Font.Family.{role}";

        /// <summary>
        /// Ключ размера шрифта для роли текста.
        /// </summary>
        /// <param name="role">Роль текста, например Body, Title, Caption.</param>
        public static string FontSize(string role) => $"Theme.Font.Size.{role}";

        /// <summary>
        /// Ключ жирности шрифта для роли текста.
        /// </summary>
        /// <param name="role">Роль текста.</param>
        public static string FontBold(string role) => $"Theme.Font.Bold.{role}";

        /// <summary>
        /// Ключ курсива шрифта для роли текста.
        /// </summary>
        /// <param name="role">Роль текста.</param>
        public static string FontItalic(string role) => $"Theme.Font.Italic.{role}";

        /// <summary>
        /// Ключ наличия зачеркивания для роли текста.
        /// </summary>
        /// <param name="role">Роль текста.</param>
        public static string FontStrikethrough(string role) => $"Theme.Font.Strikethrough.{role}";

        /// <summary>
        /// Ключ наличия подчеркивания для роли текста.
        /// </summary>
        /// <param name="role">Роль текста.</param>
        public static string FontUnderline(string role) => $"Theme.Font.Underline.{role}";

        /// <summary>
        /// Ключ стиля зачеркивания для роли текста.
        /// </summary>
        /// <param name="role">Роль текста.</param>
        public static string FontStrikethroughStyle(string role) => $"Theme.Font.StrikethroughStyle.{role}";

        /// <summary>
        /// Ключ стиля подчеркивания для роли текста.
        /// </summary>
        /// <param name="role">Роль текста.</param>
        public static string FontUnderlineStyle(string role) => $"Theme.Font.UnderlineStyle.{role}";

        /// <summary>
        /// Ключ глобальной геометрической настройки.
        /// </summary>
        /// <param name="property">Имя свойства, например CornerRadius, Padding.</param>
        public static string Geometry(string property) => $"Theme.Geometry.{property}";

        /// <summary>
        /// Ключ глобального эффекта.
        /// </summary>
        /// <param name="property">Имя свойства эффекта.</param>
        public static string Effect(string property) => $"Theme.Effect.{property}";

        /// <summary>
        /// Префикс ресурсов конкретного контрола в конкретном состоянии.
        /// </summary>
        /// <param name="controlType">Тип контрола, например Button.</param>
        /// <param name="state">Состояние контрола.</param>
        public static string ControlPrefix(string controlType, ComponentState state) => $"Theme.Control.{controlType}.{state}.";

        /// <summary>
        /// Ключ ресурса конкретного контрола в конкретном состоянии.
        /// </summary>
        /// <param name="controlType">Тип контрола.</param>
        /// <param name="state">Состояние контрола.</param>
        /// <param name="property">Имя свойства, например Background, Foreground, CornerRadius.</param>
        public static string Control(string controlType, ComponentState state, string property) => ControlPrefix(controlType, state) + property;
    }
}
