using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Abstractions.Interfaces
{
    /// <summary>
    /// Контекст предпросмотра.
    /// Используется контролами, чтобы понимать, в каком режиме они находятся.
    /// </summary>
    public interface IPreviewContext
    {
        /// <summary>
        /// Является ли текущий контекст предпросмотром.
        /// </summary>
        bool IsPreview { get; }

        /// <summary>
        /// Режим предпросмотра.
        /// </summary>
        PreviewMode Mode { get; }

        /// <summary>
        /// Тема, которая отображается в предпросмотре.
        /// </summary>
        ThemeDefinition Theme { get; }
    }
}
