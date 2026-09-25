using ThemeForge.Maui.Controls.Interfaces;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// Сервис системных шрифтов.
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
            #elif ANDROID
                List<string> systemFonts = GetSystemAndroidFontFiles();

                if (systemFonts.Count > 0)
                {
                    families.AddRange(systemFonts);
                }
                else
                {
                    families.Add(global::Android.Graphics.Typeface.Default?.SystemFontFamilyName);

                    families.AddRange(
                    [
                        "sans-serif",
                        "serif",
                        "monospace",
                        "casual",
                        "cursive",
                        "sans-serif-light",
                        "sans-serif-condensed",
                        "sans-serif-thin",
                        "sans-serif-medium",
                        "Roboto",
                        "Google Sans"
                    ]);
                }
            #endif

            families.AddRange(CommonFamilies);

            List<string> result = families
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Task.FromResult<IReadOnlyList<string>>(result);
        }

    #if ANDROID
        private List<string> GetSystemAndroidFontFiles()
        {
            List<string> fontPaths = new ();
            string fontsFolder = "/system/fonts/";

            if (Directory.Exists(fontsFolder))
            {
                string[] files = Directory.GetFiles(fontsFolder, "*.*", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();

                    if (ext == ".ttf" || ext == ".otf")
                    {
                        fontPaths.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
            }

            return fontPaths;
        }
    #endif
    }
}
