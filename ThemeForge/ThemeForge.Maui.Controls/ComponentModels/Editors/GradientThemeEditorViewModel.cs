using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel редактора градиентной темы.
    /// </summary>
    public partial class GradientThemeEditorViewModel : ObservableObject
    {
        private readonly IThemeFactory themeFactory;

        [ObservableProperty]
        private ThemeDefinition? previewTheme;

        /// <summary>
        /// Вложенный редактор настроек градиента.
        /// </summary>
        public GradientSettingsEditorViewModel Settings { get; } = new();

        /// <summary>
        /// Создает ViewModel редактора градиентной темы.
        /// </summary>
        public GradientThemeEditorViewModel(IThemeFactory themeFactory)
        {
            this.themeFactory = themeFactory;
        }

        /// <summary>
        /// Собирает тему из текущих настроек.
        /// </summary>
        [RelayCommand]
        private async Task BuildAsync(CancellationToken cancellationToken)
        {
            GradientTheme gradient = Settings.BuildGradientTheme();
            List<string> hexes = gradient.Stops.Select(s => s.Color.Hex).ToList();

            PreviewTheme = await themeFactory.CreateGradientAsync(
                gradient.Type,
                hexes,
                gradient.Geometry,
                gradient.BackgroundEffect,
                cancellationToken: cancellationToken);
        }
    }
}
