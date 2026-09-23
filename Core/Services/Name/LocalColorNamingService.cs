using Abstractions.Enums;
using Abstractions.Interfaces;
using Abstractions.Records.UseOfColors;
using Core.Helpers;
using Core.Interfaces;

namespace Core.Services.Name
{
    /// <summary>
    /// Локальная реализация сервиса именования цветов.
    /// </summary>
    public sealed class LocalColorNamingService : IColorNamingService
    {
        private readonly IColorNameLookup lookup;

        /// <summary>
        /// Создает локальный сервис именования.
        /// </summary>
        public LocalColorNamingService(IColorNameLookup lookup)
        {
            this.lookup = lookup;
        }

        /// <inheritdoc />
        public ValueTask<ColorToken> GetNameAsync(string hex, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!ColorUtility.TryNormalizeHex(hex, out var normalized))
            {
                return ValueTask.FromResult(ColorToken.FromHex(hex));
            }

            string? name = lookup.Lookup(normalized);

            ColorToken token = new ()
            {
                Hex = normalized,
                Name = name,
                Source = name is null ? ColorNameSource.Hex : ColorNameSource.Local
            };

            return ValueTask.FromResult(token);
        }

        /// <inheritdoc />
        public ValueTask<IReadOnlyDictionary<string, ColorToken>> GetNamesAsync(IEnumerable<string> hexes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Dictionary<string, ColorToken> result = new (StringComparer.OrdinalIgnoreCase);

            foreach (string hex in hexes)
            {
                if (!ColorUtility.TryNormalizeHex(hex, out string normalized) || result.ContainsKey(normalized)) continue;

                string? name = lookup.Lookup(normalized);

                result[normalized] = new ColorToken
                {
                    Hex = normalized,
                    Name = name,
                    Source = name is null ? ColorNameSource.Hex : ColorNameSource.Local
                };
            }

            return ValueTask.FromResult<IReadOnlyDictionary<string, ColorToken>>(result);
        }
    }
}
