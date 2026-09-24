using ThemeForge.Abstractions.Enums;

namespace ThemeForge.Abstractions.Helpers
{
    /// <summary>
    /// Помощник для ключей компонентных тем.
    /// </summary>
    public static class ThemeComponentKey
    {
        /// <summary>
        /// Разделитель ключа.
        /// </summary>
        public const char Separator = ':';

        /// <summary>
        /// Создает ключ компонента.
        /// </summary>
        public static string Create(string controlType, ComponentState state) => string.Concat(controlType, Separator.ToString(), state.ToString());

        /// <summary>
        /// Пытается разобрать ключ компонента.
        /// </summary>
        public static bool TryParse(string key, out string controlType, out ComponentState state)
        {
            controlType = string.Empty;
            state = ComponentState.Default;

            if (string.IsNullOrWhiteSpace(key)) return false;

            string[] parts = key.Split(Separator, 2);

            if (parts.Length != 2) return false;

            controlType = parts[0];

            return Enum.TryParse(parts[1], ignoreCase: true, out state);
        }
    }
}
