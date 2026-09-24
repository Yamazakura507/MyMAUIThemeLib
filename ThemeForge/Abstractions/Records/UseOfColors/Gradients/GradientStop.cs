namespace ThemeForge.Abstractions.Records.UseOfColors.Gradients
{
    /// <summary>
    /// Одна остановка градиента.
    /// </summary>
    /// <param name="Color">Цвет остановки.</param>
    /// <param name="Offset">Позиция от 0 до 1.</param>
    public sealed record GradientStop(ColorToken Color, double Offset);
}
