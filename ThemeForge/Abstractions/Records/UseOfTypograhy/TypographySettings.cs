using ThemeForge.Abstractions.Enums;

namespace ThemeForge.Abstractions.Records.UseOfTypograhy
{
    /// <summary>
    /// Настройки текста/типографики.
    /// Все поля nullable, чтобы можно было наследовать значение из родителя или темы по умолчанию.
    /// </summary>
    public sealed record TypographySettings
    {
        /// <summary>
        /// Семейство шрифтов.
        /// </summary>
        public string? FontFamily { get; init; }

        /// <summary>
        /// Размер шрифта.
        /// </summary>
        public double? FontSize { get; init; }

        /// <summary>
        /// Жирность.
        /// </summary>
        public bool? IsBold { get; init; }

        /// <summary>
        /// Курсив.
        /// </summary>
        public bool? IsItalic { get; init; }

        /// <summary>
        /// Есть ли зачеркивание.
        /// </summary>
        public bool? HasStrikethrough { get; init; }

        /// <summary>
        /// Есть ли подчеркивание.
        /// </summary>
        public bool? HasUnderline { get; init; }

        /// <summary>
        /// Стиль зачеркивания.
        /// </summary>
        public TextDecorationStyle? StrikethroughStyle { get; init; }

        /// <summary>
        /// Стиль подчеркивания.
        /// </summary>
        public TextDecorationStyle? UnderlineStyle { get; init; }

        /// <summary>
        /// Межстрочный интервал.
        /// </summary>
        public double? LineHeight { get; init; }

        /// <summary>
        /// Межбуквенный интервал.
        /// </summary>
        public double? LetterSpacing { get; init; }
    }
}
