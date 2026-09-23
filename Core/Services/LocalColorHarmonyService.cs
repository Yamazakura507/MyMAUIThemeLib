using Abstractions.Enums;
using Abstractions.Interfaces;
using Abstractions.Records.UseOfColors;
using Core.Helpers;

namespace Core.Services
{
    /// <summary>
    /// Локальная реализация сервиса цветовых гармоний.
    /// </summary>
    public sealed class LocalColorHarmonyService : IColorHarmonyService
    {
        /// <inheritdoc />
        public ValueTask<IReadOnlyList<ColorToken>> GenerateAsync(ColorToken baseColor, ColorHarmonyMode mode, int count = 5, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<ColorToken> palette = ColorHarmonyGenerator.Generate(baseColor.Hex, mode, count);

            return ValueTask.FromResult(palette);
        }
    }
}
