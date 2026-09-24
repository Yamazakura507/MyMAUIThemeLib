namespace ThemeForge.Core.Records
{
    /// <summary>
    /// HSL/HA-представление цвета.
    /// </summary>
    /// <param name="H">Тон в градусах от 0 до 360.</param>
    /// <param name="S">Насыщенность от 0 до 1.</param>
    /// <param name="L">Светлота от 0 до 1.</param>
    /// <param name="A">Альфа-канал от 0 до 1.</param>
    public readonly record struct HslColor(double H, double S, double L, double A = 1.0);
}
