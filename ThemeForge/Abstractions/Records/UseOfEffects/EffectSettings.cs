using ThemeForge.Abstractions.Enums;

namespace ThemeForge.Abstractions.Records.UseOfEffects
{
    /// <summary>
    /// Настройки визуального эффекта.
    /// </summary>
    public sealed record EffectSettings
    {
        /// <summary>
        /// Включен ли эффект.
        /// </summary>
        public bool IsEnabled { get; init; }

        /// <summary>
        /// Тип эффекта.
        /// </summary>
        public EffectKind Kind { get; init; } = EffectKind.None;

        /// <summary>
        /// Интенсивность эффекта от 0 до 1.
        /// </summary>
        public double Intensity { get; init; } = 0.5;

        /// <summary>
        /// Скорость эффекта. 1.0 — базовая скорость.
        /// </summary>
        public double Speed { get; init; } = 1.0;

        /// <summary>
        /// Дополнительные параметры эффекта, специфичные для конкретного рендерера.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; init; } = new Dictionary<string, string>();
    }
}
