namespace ThemeForge.Core.Services
{
    /// <summary>
    /// Опции ядра ThemeForge.
    /// </summary>
    public sealed class ThemeForgeCoreOptions
    {
        /// <summary>
        /// Папка для локального хранения тем.
        /// Если пусто, используется in-memory хранилище.
        /// </summary>
        public string? StorageDirectory { get; set; }

        /// <summary>
        /// Имя файла хранилища тем.
        /// </summary>
        public string StorageFileName { get; set; } = "theme-forge-themes.json";

        /// <summary>
        /// Добавлять ли встроенные темы при первом запуске.
        /// </summary>
        public bool SeedBuiltInThemes { get; set; } = true;
    }
}
