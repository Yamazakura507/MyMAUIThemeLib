using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Core.Helpers;

namespace ThemeForge.Core.Services
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
