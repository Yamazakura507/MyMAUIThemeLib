using ThemeForge.Core.Helpers;
using ThemeForge.Core.Interfaces;
using ThemeForge.Core.Records;

namespace ThemeForge.Core.Services.Name
{
    /// <summary>
    /// Локальный поставщик имен цветов.
    /// Работает без сети и используется как fallback для API.
    /// </summary>
    public sealed class LocalColorNameProvider : IColorNameLookup
    {
        private static readonly (string Name, string Hex)[] KnownColors =
        [
            ("Black", "#000000"),
            ("White", "#FFFFFF"),
            ("Gray", "#9E9E9E"),
            ("Silver", "#C0C0C0"),
            ("Slate", "#708090"),
            ("Red", "#D32F2F"),
            ("Crimson", "#DC143C"),
            ("Orange", "#F57C00"),
            ("Amber", "#FFC107"),
            ("Yellow", "#FFEB3B"),
            ("Lime", "#AFB42B"),
            ("Green", "#2E7D32"),
            ("Teal", "#00796B"),
            ("Cyan", "#00BCD4"),
            ("Azure", "#03A9F4"),
            ("Blue", "#1976D2"),
            ("Indigo", "#303F9F"),
            ("Violet", "#7E57C2"),
            ("Purple", "#9C27B0"),
            ("Magenta", "#E91E63"),
            ("Pink", "#EC407A"),
            ("Brown", "#795548")
        ];

        private readonly List<Entry> entries;

        /// <summary>
        /// Создает локальный провайдер имен.
        /// </summary>
        /// <param name="maxDistance">Максимальное RGB-расстояние для принятия имени.</param>
        public LocalColorNameProvider(double maxDistance = 150.0)
        {
            MaxDistance = maxDistance;
            entries = [];

            foreach ((string name, string hex) in KnownColors)
            {
                if (ColorUtility.TryParseArgb(hex, out byte a, out byte r, out byte g, out byte b))
                {
                    entries.Add(new Entry(name, a, r, g, b));
                }
            }
        }

        /// <summary>
        /// Максимальное расстояние до известного цвета.
        /// </summary>
        public double MaxDistance { get; }

        /// <inheritdoc />
        public string? Lookup(string hex)
        {
            if (!ColorUtility.TryParseArgb(hex, out byte a, out byte r, out byte g, out byte b))
            {
                return null;
            }

            string? bestName = null;
            double bestDistance = double.MaxValue;

            foreach (Entry entry in entries)
            {
                if (entry.A != a) continue;

                double dr = entry.R - r;
                double dg = entry.G - g;
                double db = entry.B - b;

                double distance = Math.Sqrt((dr * dr) + (dg * dg) + (db * db));

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestName = entry.Name;
                }
            }

            return bestDistance <= MaxDistance ? bestName : null;
        }
    }
}
