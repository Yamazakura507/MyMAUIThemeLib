using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Specialized;
using System.ComponentModel;
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

            AttachNestedNotifications();
        }

        /// <summary>
        /// Собирает тему из текущих настроек.
        /// </summary>
        [RelayCommand]
        private async Task BuildAsync(CancellationToken cancellationToken)
        {
            GradientTheme gradient = Settings.BuildGradientTheme();

            List<string> hexes = gradient.Stops.Select(s => s.Color.Hex).ToList();

            PreviewTheme = await themeFactory.CreateGradientAsync(gradient.Type, hexes, gradient.Geometry, gradient.BackgroundEffect, cancellationToken: cancellationToken);
        }

        private void AttachNestedNotifications()
        {
            Settings.PropertyChanged += OnNestedPropertyChanged;
            Settings.EffectEditor.PropertyChanged += OnNestedPropertyChanged;
            Settings.Stops.CollectionChanged += OnStopsCollectionChanged;

            foreach (GradientStopViewModel stop in Settings.Stops)
            {
                stop.PropertyChanged += OnNestedPropertyChanged;
            }
        }

        private void OnStopsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += OnNestedPropertyChanged;
                }
            }

            if (e.OldItems is not null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    item.PropertyChanged -= OnNestedPropertyChanged;
                }
            }

            OnPropertyChanged(nameof(Settings));
        }

        private void OnNestedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Settings));
        }
    }
}
