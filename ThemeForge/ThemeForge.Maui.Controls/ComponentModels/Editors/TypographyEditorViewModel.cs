using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfTypograhy;
using ThemeForge.Maui.Controls.Interfaces;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel редактора типографики.
    /// </summary>
    public partial class TypographyEditorViewModel : ObservableObject
    {
        private readonly IFontCatalogService fontCatalogService;

        [ObservableProperty]
        private string? selectedFontFamily;

        [ObservableProperty]
        private double fontSize = 14;

        [ObservableProperty]
        private bool isBold;

        [ObservableProperty]
        private bool isItalic;

        [ObservableProperty]
        private bool hasStrikethrough;

        [ObservableProperty]
        private bool hasUnderline;

        [ObservableProperty]
        private TextDecorationStyle strikethroughStyle = TextDecorationStyle.Solid;

        [ObservableProperty]
        private TextDecorationStyle underlineStyle = TextDecorationStyle.Solid;

        /// <summary>
        /// Минимально допустимый размер шрифта.
        /// </summary>
        public double MinFontSize { get; } = 8;

        /// <summary>
        /// Максимально допустимый размер шрифта.
        /// </summary>
        public double MaxFontSize { get; } = 32;

        /// <summary>
        /// Доступные семейства шрифтов.
        /// </summary>
        public IReadOnlyList<string> FontFamilies { get; private set; } = [];

        /// <summary>
        /// Доступные стили украшений текста.
        /// </summary>
        public Array DecorationStyles { get; } = Enum.GetValues(typeof(TextDecorationStyle));

        /// <summary>
        /// Создает ViewModel редактора типографики.
        /// </summary>
        public TypographyEditorViewModel(IFontCatalogService fontCatalogService)
        {
            this.fontCatalogService = fontCatalogService;
        }

        /// <summary>
        /// Загружает список шрифтов.
        /// </summary>
        [RelayCommand]
        private async Task LoadFontsAsync(CancellationToken cancellationToken)
        {
            FontFamilies = await fontCatalogService.GetFamiliesAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(SelectedFontFamily) && FontFamilies.Count > 0)
            {
                SelectedFontFamily = FontFamilies[0];
            }
        }

        /// <summary>
        /// Строит настройки типографики.
        /// </summary>
        public TypographySettings BuildSettings() => new TypographySettings
        {
            FontFamily = SelectedFontFamily,
            FontSize = Math.Clamp(FontSize, MinFontSize, MaxFontSize),
            IsBold = IsBold,
            IsItalic = IsItalic,
            HasStrikethrough = HasStrikethrough,
            HasUnderline = HasUnderline,
            StrikethroughStyle = StrikethroughStyle,
            UnderlineStyle = UnderlineStyle
        };

        /// <summary>
        /// Загружает настройки из существующей типографики.
        /// </summary>
        public void LoadFrom(TypographySettings? settings)
        {
            SelectedFontFamily = settings?.FontFamily;
            FontSize = Math.Clamp(settings?.FontSize ?? 14, MinFontSize, MaxFontSize);
            IsBold = settings?.IsBold ?? false;
            IsItalic = settings?.IsItalic ?? false;
            HasStrikethrough = settings?.HasStrikethrough ?? false;
            HasUnderline = settings?.HasUnderline ?? false;
            StrikethroughStyle = settings?.StrikethroughStyle ?? TextDecorationStyle.Solid;
            UnderlineStyle = settings?.UnderlineStyle ?? TextDecorationStyle.Solid;
        }
    }
}
