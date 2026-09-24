namespace ThemeForge.Maui.Controls.Interfaces
{
    /// <summary>
    /// Сервис получения списка семейств шрифтов.
    /// </summary>
    public interface IFontCatalogService
    {
        /// <summary>
        /// Возвращает доступные семейства шрифтов.
        /// </summary>
        Task<IReadOnlyList<string>> GetFamiliesAsync(CancellationToken cancellationToken = default);
    }
}
