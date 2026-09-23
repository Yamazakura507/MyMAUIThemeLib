using Abstractions.Records.UseOfTheme;

namespace Core.Services.Storage
{
    /// <summary>
    /// Документ хранения тем.
    /// </summary>
    public sealed class ThemeStorageDocument
    {
        /// <summary>
        /// Версия схемы документа.
        /// </summary>
        public int SchemaVersion { get; set; } = 1;

        /// <summary>
        /// Список тем.
        /// </summary>
        public List<ThemeDefinition> Themes { get; set; } = [];
    }
}
