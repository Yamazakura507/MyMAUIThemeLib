namespace Abstractions.Records.UseOfColors
{
    /// <summary>
    /// Нормализованная точка в координатах 0..1.
    /// Используется для градиентов, чтобы не зависеть от pixel-размеров.
    /// </summary>
    /// <param name="X">Горизонтальная координата от 0 до 1.</param>
    /// <param name="Y">Вертикальная координата от 0 до 1.</param>
    public readonly record struct NormalizedPoint(double X, double Y);
}
