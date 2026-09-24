using ThemeForge.Abstractions.EventArgs;
using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Abstractions.Interfaces
{
    /// <summary>
    /// Центральный сервис тем.
    /// Управляет текущей примененной темой, черновиком/preview и пресетами.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Текущая примененная тема.
        /// </summary>
        ThemeDefinition Current { get; }

        /// <summary>
        /// Текущий черновик, который отображается в предпросмотре до сохранения/применения.
        /// </summary>
        ThemeDefinition Draft { get; }

        /// <summary>
        /// Есть ли несохраненные изменения в черновике.
        /// </summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        /// Событие применения темы к реальному интерфейсу.
        /// </summary>
        event EventHandler<ThemeChangedEventArgs> Applied;

        /// <summary>
        /// Событие изменения черновика/preview.
        /// </summary>
        event EventHandler<ThemeChangedEventArgs> DraftChanged;

        /// <summary>
        /// Возвращает список пресетов.
        /// </summary>
        ValueTask<IReadOnlyList<ThemeDefinition>> GetPresetsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Выбирает пресет как черновик для preview.
        /// </summary>
        ValueTask SelectPresetAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Применяет текущий черновик к интерфейсу.
        /// </summary>
        ValueTask ApplyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохраняет текущий черновик как пользовательскую тему.
        /// </summary>
        ValueTask SaveDraftAsCustomAsync(string? name = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет черновик.
        /// </summary>
        ValueTask UpdateDraftAsync(ThemeDefinition draft, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сбрасывает черновик к текущей примененной теме.
        /// </summary>
        ValueTask ResetDraftAsync(CancellationToken cancellationToken = default);
    }
}
