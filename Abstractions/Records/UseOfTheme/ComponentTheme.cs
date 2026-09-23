using Abstractions.Enums;
using Abstractions.Records.UseOfColors;
using Abstractions.Records.UseOfEffects;
using Abstractions.Records.UseOfGeometry;
using Abstractions.Records.UseOfTypograhy;

namespace Abstractions.Records.UseOfTheme
{
    /// <summary>
    /// Тема одного компонента в определенном состоянии.
    /// </summary>
    /// <param name="ControlType">Тип контрола, например Button, Slider, Switch.</param>
    public sealed record ComponentTheme(string ControlType)
    {
        /// <summary>
        /// Состояние компонента.
        /// </summary>
        public ComponentState State { get; init; } = ComponentState.Default;

        /// <summary>
        /// Цвет переднего плана.
        /// </summary>
        public ColorToken? Foreground { get; init; }

        /// <summary>
        /// Цвет фона.
        /// </summary>
        public ColorToken? Background { get; init; }

        /// <summary>
        /// Цвет границы.
        /// </summary>
        public ColorToken? Border { get; init; }

        /// <summary>
        /// Градиентный фон, если для компонента доступен градиент.
        /// </summary>
        public GradientTheme? BackgroundGradient { get; init; }

        /// <summary>
        /// Типографика компонента.
        /// </summary>
        public TypographySettings? Typography { get; init; }

        /// <summary>
        /// Геометрия компонента.
        /// </summary>
        public GeometrySettings? Geometry { get; init; }

        /// <summary>
        /// Эффекты компонента.
        /// </summary>
        public EffectSettings? Effects { get; init; }

        /// <summary>
        /// Дополнительные именованные цвета компонента, например Track, Thumb, Indicator, Ripple.
        /// </summary>
        public IReadOnlyDictionary<string, ColorToken>? NamedColors { get; init; }
    }
}
