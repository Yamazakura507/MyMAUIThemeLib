using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel редактора простой цветовой темы.
    /// </summary>
    public partial class SolidThemeEditorViewModel : ObservableObject
    {
        private readonly IThemeFactory themeFactory;

        [ObservableProperty]
        private ColorHarmonyMode harmony = ColorHarmonyMode.Monochrome;

        [ObservableProperty]
        private string baseHex = "#2E7D32";

        [ObservableProperty]
        private ThemeDefinition? previewTheme;

        /// <summary>
        /// Доступные режимы гармонии.
        /// </summary>
        public Array Harmonies { get; } = Enum.GetValues(typeof(ColorHarmonyMode));

        /// <summary>
        /// Палитра, полученная из выбранной гармонии.
        /// </summary>
        public ObservableCollection<ColorToken> Palette { get; } = [];

        /// <summary>
        /// Создает ViewModel редактора простой темы.
        /// </summary>
        public SolidThemeEditorViewModel(IThemeFactory themeFactory)
        {
            this.themeFactory = themeFactory;
        }

        /// <summary>
        /// Генерирует простую тему и палитру.
        /// </summary>
        [RelayCommand]
        private async Task GenerateAsync(CancellationToken cancellationToken)
        {
            PreviewTheme = await themeFactory.CreateSolidAsync(Harmony, BaseHex, cancellationToken: cancellationToken);

            Palette.Clear();

            if (PreviewTheme?.Solid is not null)
            {
                foreach (ColorToken color in PreviewTheme.Solid.Palette)
                {
                    Palette.Add(color);
                }
            }
        }
    }
}
