namespace Abstractions.Enums
{
    /// <summary>
    /// Режим цветовой гармонии для простых тем.
    /// </summary>
    public enum ColorHarmonyMode
    {
        /// <summary>
        /// Оттенки одного цвета.
        /// </summary>
        Monochrome,

        /// <summary>
        /// Соседние цвета на цветовом круге.
        /// </summary>
        Analogous,

        /// <summary>
        /// Дополнительный цвет.
        /// </summary>
        Complementary,

        /// <summary>
        /// Триадическая гармония.
        /// </summary>
        Triadic,

        /// <summary>
        /// Тетрадическая/квадратная гармония.
        /// </summary>
        Tetradic
    }

}
