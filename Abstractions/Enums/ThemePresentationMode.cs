using System;
using System.Collections.Generic;
using System.Text;

namespace Abstractions.Enums
{
    /// <summary>
    /// Режим отображения карточки темы в списке готовых тем.
    /// </summary>
    public enum ThemePresentationMode
    {
        /// <summary>
        /// Автоматически выбрать подходящий вид.
        /// </summary>
        Automatic,

        /// <summary>
        /// Показывать как простую цветовую тему.
        /// </summary>
        Solid,

        /// <summary>
        /// Показывать как линейный градиент.
        /// </summary>
        LinearGradient,

        /// <summary>
        /// Показывать как радиальный градиент.
        /// </summary>
        RadialGradient,

        /// <summary>
        /// Показывать с анимированным эффектом, если он есть.
        /// </summary>
        Animated
    }
}
