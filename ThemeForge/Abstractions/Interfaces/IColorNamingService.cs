using ThemeForge.Abstractions.Records.UseOfColors;

namespace ThemeForge.Abstractions.Interfaces
{
    /// <summary>
    /// Сервис получения имен цветов.
    /// Реализация может быть локальной, кэшированной или сетевой.
    /// </summary>
    public interface IColorNamingService
    {
        /// <summary>
        /// Получает имя для одного hex-цвета.
        /// </summary>
        ValueTask<ColorToken> GetNameAsync(string hex, CancellationToken cancellationToken = default);

        /// <summary>
        /// Пачкой получает имена для нескольких цветов.
        /// Ключ результата — нормализованный hex.
        /// </summary>
        ValueTask<IReadOnlyDictionary<string, ColorToken>> GetNamesAsync(IEnumerable<string> hexes, CancellationToken cancellationToken = default);
    }
}
