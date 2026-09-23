namespace Abstractions.Records.UseOfColors.Gradients
{
    /// <summary>
    /// Геометрия градиента.
    /// </summary>
    public sealed record GradientGeometry
    {
        /// <summary>
        /// Угол линейного градиента в градусах.
        /// </summary>
        public double AngleDegrees { get; init; }

        /// <summary>
        /// Начальная точка для линейного градиента.
        /// </summary>
        public NormalizedPoint? StartPoint { get; init; }

        /// <summary>
        /// Конечная точка для линейного градиента.
        /// </summary>
        public NormalizedPoint? EndPoint { get; init; }

        /// <summary>
        /// Центральная точка для радиального градиента.
        /// </summary>
        public NormalizedPoint? CenterPoint { get; init; }

        /// <summary>
        /// Радиус радиального градиента в нормализованных координатах.
        /// </summary>
        public double Radius { get; init; } = 0.5;

        /// <summary>
        /// Фокальный радиус, если поддерживается платформой/рендерером.
        /// </summary>
        public double FocalRadius { get; init; }

        /// <summary>
        /// Фокальный угол, если поддерживается платформой/рендерером.
        /// </summary>
        public double FocalAngleDegrees { get; init; }
    }
}
