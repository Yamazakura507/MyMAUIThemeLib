using ThemeForge.Core.Helpers;
using System.Text.Json;

namespace ThemeForge.Services.TheColor.Helpers
{
    /// <summary>
    /// Вспомогательные методы для гибкого чтения JSON-ответов TheColor API.
    /// </summary>
    internal static class TheColorJsonHelpers
    {
        /// <summary>
        /// Читает строковое значение из объекта по одному из возможных имен свойств.
        /// </summary>
        public static string? GetStringOrNull(this JsonElement element, params string[] propertyNames)
        {
            if (element.ValueKind != JsonValueKind.Object) return null;

            foreach (string propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String)
                {
                    string? text = value.GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Извлекает HEX-цвет из элемента, который может быть строкой или объектом.
        /// </summary>
        public static string? GetHexOrNull(this JsonElement element)
        {
            string? text = element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Object => GetStringOrNull(element, "value", "hex", "code"),
                _ => null
            };

            return ColorUtility.TryNormalizeHex(text, out string normalized) ? normalized : null;
        }

        /// <summary>
        /// Извлекает имя цвета из элемента, который может быть строкой или объектом.
        /// </summary>
        public static string? GetNameOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String && element.GetString() is { } text && !string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (GetStringOrNull(element, "value", "name") is { } value && !string.IsNullOrWhiteSpace(value)) return value;
                if (GetStringOrNull(element, "closest") is { } closest && !string.IsNullOrWhiteSpace(closest)) return closest;
            }

            return null;
        }

        /// <summary>
        /// Пробует прочитать массив цветов из разных возможных форм ответа.
        /// </summary>
        public static bool TryGetColorsArray(this JsonElement root, out JsonElement colors)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                colors = root;

                return true;
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (string? propertyName in new[] { "colors", "data", "items", "palette" })
                {
                    if (root.TryGetProperty(propertyName, out JsonElement candidate) && candidate.ValueKind == JsonValueKind.Array)
                    {
                        colors = candidate;

                        return true;
                    }
                }
            }

            colors = default;

            return false;
        }
    }
}
