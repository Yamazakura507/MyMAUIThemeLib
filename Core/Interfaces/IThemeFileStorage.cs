namespace Core.Interfaces
{
    /// <summary>
    /// Абстракция файлового хранилища для JSON-тем.
    /// </summary>
    public interface IThemeFileStorage
    {
        /// <summary>
        /// Читает все содержимое файла.
        /// </summary>
        ValueTask<string?> ReadAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Записывает все содержимое файла.
        /// </summary>
        ValueTask WriteAllAsync(string contents, CancellationToken cancellationToken = default);
    }
}
