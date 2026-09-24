using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Abstractions.Interfaces
{
    /// <summary>
    /// Хранилище тем/пресетов.
    /// </summary>
    public interface IThemeRepository
    {
        /// <summary>
        /// Загружает все доступные темы: встроенные, пользовательские, избранные.
        /// </summary>
        ValueTask<IReadOnlyList<ThemeDefinition>> LoadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохраняет или обновляет тему.
        /// </summary>
        ValueTask SaveAsync(ThemeDefinition theme, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет тему по идентификатору.
        /// </summary>
        ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ищет тему по идентификатору.
        /// </summary>
        ValueTask<ThemeDefinition?> FindAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Происходит при изменении хранилища.
        /// </summary>
        event EventHandler? Changed;
    }
}
