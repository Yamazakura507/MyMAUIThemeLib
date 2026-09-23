using Abstractions.Enums;
using Abstractions.Records.UseOfColors.Gradients;
using Abstractions.Records.UseOfEffects;

namespace Abstractions.Records.UseOfTheme
{
    /// <summary>
    /// Градиентная тема.
    /// </summary>
    /// <param name="Type">Тип градиента.</param>
    /// <param name="Stops">Остановки градиента, +- от 2 до 4 цветов в обычном UI.</param>
    public sealed record GradientTheme(GradientType Type, IReadOnlyList<GradientStop> Stops)
    {
        /// <summary>
        /// Геометрия градиента.
        /// </summary>
        public GradientGeometry Geometry { get; init; } = new();

        /// <summary>
        /// Фоновый эффект градиента, например волна или перемещение цветов.
        /// </summary>
        public EffectSettings? BackgroundEffect { get; init; }
    }
}
