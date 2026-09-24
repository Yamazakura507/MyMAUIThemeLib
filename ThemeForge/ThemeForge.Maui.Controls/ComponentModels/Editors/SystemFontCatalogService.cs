using ThemeForge.Maui.Controls.Interfaces;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// Сервис системных шрифтов.
    /// На Android используется расширенный fallback-список, потому что публичного кроссплатформенного
    /// перечисления всех шрифтов нет.
    /// </summary>
    public sealed class SystemFontCatalogService : IFontCatalogService
    {
        private static readonly string[] CommonFamilies =
        [
            "Default",
            "Arial",
            "Helvetica",
            "Segoe UI",
            "Roboto",
            "San Francisco",
            "Open Sans",
            "Lato",
            "Montserrat",
            "Inter"
        ];

        /// <inheritdoc />
        public Task<IReadOnlyList<string>> GetFamiliesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<string> families = new ();

            #if IOS || MACCATALYST
                families.AddRange(UIKit.UIFont.FamilyNames);
            #elif WINDOWS
                families.AddRange(Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies());
            #endif
           
            List<string> result = families
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Task.FromResult<IReadOnlyList<string>>(result);
        }
    }
}
