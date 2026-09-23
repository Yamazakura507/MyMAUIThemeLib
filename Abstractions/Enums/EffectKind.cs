namespace Abstractions.Enums
{
    /// <summary>
    /// Тип фонового/визуального эффекта.
    /// </summary>
    public enum EffectKind
    {
        /// <summary>
        /// Эффект отсутствует.
        /// </summary>
        None,

        /// <summary>
        /// Волновой эффект.
        /// </summary>
        Wave,

        /// <summary>
        /// Перемещение цветов градиента.
        /// </summary>
        MovingColors,

        /// <summary>
        /// Мерцание/блик.
        /// </summary>
        Shimmer,

        /// <summary>
        /// Шум/грануляция.
        /// </summary>
        Noise,

        /// <summary>
        /// Lottie-анимация.
        /// </summary>
        Lottie
    }
}
