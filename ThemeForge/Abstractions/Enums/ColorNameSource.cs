namespace ThemeForge.Abstractions.Enums
{
    /// <summary>
    /// Источник имени цвета.
    /// </summary>
    public enum ColorNameSource
    {
        /// <summary>
        /// Имя не определено, используется hex.
        /// </summary>
        Hex,

        /// <summary>
        /// Имя получено из локального справочника.
        /// </summary>
        Local,

        /// <summary>
        /// Имя получено из кэша API.
        /// </summary>
        Cache,

        /// <summary>
        /// Имя получено из внешнего API, например TheColor.
        /// </summary>
        Api,

        /// <summary>
        /// Имя задано пользователем вручную.
        /// </summary>
        User
    }
}
