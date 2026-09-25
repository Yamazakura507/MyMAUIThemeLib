using System;
using System.Collections.Generic;
using System.Text;

namespace ThemeForge.Abstractions.Knowledge
{
    /// <summary>
    /// Известные ключи параметров для эффектов.
    /// </summary>
    public static class KnownEffectParameters
    {
        /// <summary>
        /// Количество волновых полос для эффекта Wave.
        /// </summary>
        public const string WaveBands = "Wave.Bands";

        /// <summary>
        /// Множитель частоты волны для эффекта Wave.
        /// </summary>
        public const string WaveFrequency = "Wave.Frequency";

        /// <summary>
        /// Множитель амплитуды волны для эффекта Wave.
        /// </summary>
        public const string WaveAmplitude = "Wave.Amplitude";

        /// <summary>
        /// Горизонтальное направление движения цветов для эффекта MovingColors.
        /// </summary>
        public const string MovingColorsDirectionX = "MovingColors.DirectionX";

        /// <summary>
        /// Вертикальное направление движения цветов для эффекта MovingColors.
        /// </summary>
        public const string MovingColorsDirectionY = "MovingColors.DirectionY";

        /// <summary>
        /// Зацикливать ли движение цветов.
        /// </summary>
        public const string MovingColorsWrap = "MovingColors.Wrap";

        /// <summary>
        /// Угол блика для эффекта Shimmer.
        /// </summary>
        public const string ShimmerAngleDegrees = "Shimmer.AngleDegrees";

        /// <summary>
        /// Относительная ширина блика для эффекта Shimmer.
        /// </summary>
        public const string ShimmerWidthRatio = "Shimmer.WidthRatio";

        /// <summary>
        /// Множитель прозрачности блика для эффекта Shimmer.
        /// </summary>
        public const string ShimmerOpacityMultiplier = "Shimmer.OpacityMultiplier";

        /// <summary>
        /// Количество октав шума для эффекта Noise.
        /// </summary>
        public const string NoiseOctaves = "Noise.Octaves";

        /// <summary>
        /// Множитель частоты шума для эффекта Noise.
        /// </summary>
        public const string NoiseFrequency = "Noise.Frequency";

        /// <summary>
        /// Контраст/интенсивность шума для эффекта Noise.
        /// </summary>
        public const string NoiseContrast = "Noise.Contrast";
    }
}
