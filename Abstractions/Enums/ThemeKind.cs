namespace Abstractions.Enums
{
    /// <summary>
    /// Общий вид темы: однотонная цветовая тема или градиентная тема.
    /// </summary>
    public enum ThemeKind
    {
        /// <summary>
        /// Простая цветовая тема на основе одного базового цвета и гармонии.
        /// </summary>
        Solid,

        /// <summary>
        /// Линейный градиент.
        /// </summary>
        LinearGradient,

        /// <summary>
        /// Радиальный градиент.
        /// </summary>
        RadialGradient
    }
}
