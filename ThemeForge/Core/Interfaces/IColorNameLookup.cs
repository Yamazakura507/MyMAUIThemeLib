namespace ThemeForge.Core.Interfaces
{
    /// <summary>
    /// Синхронный поиск имени цвета по HEX.
    /// Используется для мгновенного offline-preview.
    /// </summary>
    public interface IColorNameLookup
    {
        /// <summary>
        /// Возвращает ближайшее известное имя цвета или <c>null</c>.
        /// </summary>
        /// <param name="hex">HEX-цвет.</param>
        string? Lookup(string hex);
    }
}
