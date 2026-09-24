using ThemeForge.Abstractions.Enums;

namespace ThemeForge.Abstractions.Records.UseOfColors
{
    /// <summary>
    /// Цвет с возможным именем и источником имени.
    /// </summary>
    public sealed record ColorToken
    {
        /// <summary>
        /// HEX-значение цвета.
        /// </summary>
        public string Hex { get; init; } = "#000000";

        /// <summary>
        /// Человекочитаемое имя цвета, если известно.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Источник имени цвета.
        /// </summary>
        public ColorNameSource Source { get; init; } = ColorNameSource.Hex;

        /// <summary>
        /// Создает цветовой токен из hex без имени.
        /// </summary>
        public static ColorToken FromHex(string hex) => new()
        {
            Hex = hex,
            Source = ColorNameSource.Hex
        };
    }
}
