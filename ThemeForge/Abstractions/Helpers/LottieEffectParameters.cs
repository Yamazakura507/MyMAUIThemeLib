using System;
using System.Collections.Generic;
using System.Text;
using ThemeForge.Abstractions.Enums;

namespace ThemeForge.Abstractions.Helpers
{
    /// <summary>
    /// Известные ключи параметров для <see cref="EffectKind.Lottie"/>.
    /// </summary>
    public static class LottieEffectParameters
    {
        /// <summary>
        /// Локальный asset, например "Animations/loading.json".
        /// </summary>
        public const string AssetName = "Lottie.AssetName";

        /// <summary>
        /// URL Lottie-анимации, например "https://example.com/animation.json".
        /// </summary>
        public const string Url = "Lottie.Url";

        /// <summary>
        /// Повторять ли анимацию. Значение: "true" или "false".
        /// </summary>
        public const string Loop = "Lottie.Loop";

        /// <summary>
        /// Запускать ли анимацию автоматически. Значение: "true" или "false".
        /// </summary>
        public const string AutoPlay = "Lottie.AutoPlay";

        /// <summary>
        /// Скорость воспроизведения Lottie.
        /// </summary>
        public const string Speed = "Lottie.Speed";

        /// <summary>
        /// HEX-цвет перекраски Lottie, например "#FFFFFF".
        /// </summary>
        public const string TintColorHex = "Lottie.TintColorHex";

        /// <summary>
        /// Режим масштабирования: AspectFit, AspectFill, Zoom, Uniform и т.п.
        /// </summary>
        public const string ScaleMode = "Lottie.ScaleMode";
    }
}
