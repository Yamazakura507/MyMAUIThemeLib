using Abstractions.Enums;
using Abstractions.Records.UseOfColors;

namespace Abstractions.Interfaces
{
    /// <summary>
    /// Сервис генерации цветовых гармоний.
    /// </summary>
    public interface IColorHarmonyService
    {
        /// <summary>
        /// Генерирует палитру на основе базового цвета и режима гармонии.
        /// </summary>
        ValueTask<IReadOnlyList<ColorToken>> GenerateAsync(ColorToken baseColor, ColorHarmonyMode mode, int count = 5, CancellationToken cancellationToken = default);
    }

}
