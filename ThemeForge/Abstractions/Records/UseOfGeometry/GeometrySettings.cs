namespace ThemeForge.Abstractions.Records.UseOfGeometry
{
    /// <summary>
    /// Геометрические настройки элемента.
    /// Все поля nullable для каскадного наследования.
    /// </summary>
    public sealed record GeometrySettings
    {
        /// <summary>
        /// Ширина.
        /// </summary>
        public double? Width { get; init; }

        /// <summary>
        /// Высота.
        /// </summary>
        public double? Height { get; init; }

        /// <summary>
        /// Радиус скругления.
        /// </summary>
        public double? CornerRadius { get; init; }

        /// <summary>
        /// Толщина границы.
        /// </summary>
        public double? BorderThickness { get; init; }

        /// <summary>
        /// Внутренний отступ.
        /// </summary>
        public double? Padding { get; init; }

        /// <summary>
        /// Внешний отступ.
        /// </summary>
        public double? Margin { get; init; }

        /// <summary>
        /// Диаметр ползунка для Slider-like контролов.
        /// </summary>
        public double? ThumbDiameter { get; init; }

        /// <summary>
        /// Толщина дорожки для Slider/ProgressBar-like контролов.
        /// </summary>
        public double? TrackThickness { get; init; }

        /// <summary>
        /// Размер индикатора, например для Switch/CheckBox/RadioButton.
        /// </summary>
        public double? IndicatorSize { get; init; }

        /// <summary>
        /// Включена ли тень.
        /// </summary>
        public bool? HasShadow { get; init; }

        /// <summary>
        /// Радиус тени.
        /// </summary>
        public double? ShadowRadius { get; init; }

        /// <summary>
        /// Прозрачность тени от 0 до 1.
        /// </summary>
        public double? ShadowOpacity { get; init; }

        /// <summary>
        /// Платформенная высота/elevation, где применимо.
        /// </summary>
        public double? Elevation { get; init; }

        /// <summary>
        /// Поворот элемента.
        /// </summary>
        public double? Rotation { get; init; }

        /// <summary>
        /// Масштаб элемента.
        /// </summary>
        public double? Scale { get; init; }
    }
}
