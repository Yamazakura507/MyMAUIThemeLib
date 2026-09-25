using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Helpers;
using ThemeForge.Abstractions.Knowledge;
using ThemeForge.Abstractions.Records.UseOfEffects;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// Переиспользуемая ViewModel для редактирования <see cref="EffectSettings"/>.
    /// </summary>
    public partial class EffectSettingsEditorViewModel : ObservableObject
    {
        private bool isLoading;

        [ObservableProperty]
        private bool isEnabled;

        [ObservableProperty]
        private EffectKind kind = EffectKind.None;

        [ObservableProperty]
        private double intensity = 0.5;

        [ObservableProperty]
        private double speed = 1.0;

        [ObservableProperty]
        private string? selectedSuggestion;

        /// <summary>
        /// Параметры эффекта.
        /// </summary>
        public ObservableCollection<EffectParameterViewModel> Parameters { get; } = [];

        /// <summary>
        /// Доступные типы эффектов.
        /// </summary>
        public Array EffectKinds { get; } = Enum.GetValues(typeof(EffectKind));

        /// <summary>
        /// Рекомендуемые параметры для текущего типа эффекта.
        /// </summary>
        public IReadOnlyList<string> Suggestions => GetSuggestions(Kind);

        /// <summary>
        /// Текущее состояние эффекта, собранное из редактора.
        /// </summary>
        public EffectSettings CurrentEffect => BuildEffectSettings();

        /// <summary>
        /// Brush для превью эффекта.
        /// </summary>
        public Brush PreviewBrush { get; } = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops =
            {
                new GradientStop(Color.FromRgb(25, 118, 210), 0),
                new GradientStop(Color.FromRgb(126, 87, 194), 0.5f),
                new GradientStop(Color.FromRgb(236, 64, 122), 1)
            }
        };

        /// <summary>
        /// Создает ViewModel редактора эффектов.
        /// </summary>
        public EffectSettingsEditorViewModel()
        {
            Parameters.CollectionChanged += OnParametersCollectionChanged;
        }

        partial void OnIsEnabledChanged(bool value) => RaiseCurrentEffectChanged();

        partial void OnKindChanged(EffectKind value)
        {
            OnPropertyChanged(nameof(Suggestions));
            RaiseCurrentEffectChanged();
        }

        partial void OnIntensityChanged(double value) => RaiseCurrentEffectChanged();

        partial void OnSpeedChanged(double value) => RaiseCurrentEffectChanged();

        /// <summary>
        /// Загружает редактор из существующих настроек эффекта.
        /// </summary>
        public void LoadFrom(EffectSettings? effect)
        {
            isLoading = true;

            try
            {
                Parameters.Clear();

                IsEnabled = effect?.IsEnabled ?? false;
                Kind = effect?.Kind ?? EffectKind.None;
                Intensity = effect?.Intensity ?? 0.5;
                Speed = effect?.Speed ?? 1.0;

                if (effect?.Parameters is not null)
                {
                    foreach (KeyValuePair<string,string> pair in effect.Parameters)
                    {
                        Parameters.Add(CreateParameter(pair.Key, pair.Value));
                    }
                }
            }
            finally
            {
                isLoading = false;
            }

            RaiseCurrentEffectChanged();
        }

        /// <summary>
        /// Строит модель эффекта из текущих значений редактора.
        /// </summary>
        public EffectSettings BuildEffectSettings()
        {
            Dictionary<string, string> parameters = new (StringComparer.OrdinalIgnoreCase);

            foreach (var parameter in Parameters)
            {
                if (string.IsNullOrWhiteSpace(parameter.Key)) continue;

                parameters[parameter.Key.Trim()] = parameter.Value ?? string.Empty;
            }

            return new EffectSettings
            {
                IsEnabled = IsEnabled,
                Kind = Kind,
                Intensity = Math.Clamp(Intensity, 0.0, 1.0),
                Speed = Math.Clamp(Speed, 0.1, 10.0),
                Parameters = parameters
            };
        }

        /// <summary>
        /// Добавляет выбранный рекомендуемый параметр.
        /// </summary>
        [RelayCommand]
        private void AddSelectedSuggestion()
        {
            if (string.IsNullOrWhiteSpace(SelectedSuggestion)) return;

            AddParameter(SelectedSuggestion);
            SelectedSuggestion = null;
        }

        /// <summary>
        /// Добавляет параметр по ключу.
        /// </summary>
        [RelayCommand]
        private void AddParameter(string? key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            string trimmed = key.Trim();

            if (Parameters.Any(p => string.Equals(p.Key, trimmed, StringComparison.OrdinalIgnoreCase))) return;

            Parameters.Add(CreateParameter(trimmed, GetDefaultValue(trimmed)));
        }

        private EffectParameterViewModel CreateParameter(string key, string value)
        {
            EffectParameterViewModel item = new ()
            {
                Key = key,
                Value = value
            };

            item.RemoveCommand = new Command(() => Parameters.Remove(item));

            return item;
        }

        private void RaiseCurrentEffectChanged()
        {
            if (isLoading) return;

            OnPropertyChanged(nameof(CurrentEffect));
        }

        private void OnParametersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (EffectParameterViewModel item in e.NewItems)
                {
                    item.PropertyChanged += OnParameterPropertyChanged;
                }
            }

            if (e.OldItems is not null)
            {
                foreach (EffectParameterViewModel item in e.OldItems)
                {
                    item.PropertyChanged -= OnParameterPropertyChanged;
                }
            }

            RaiseCurrentEffectChanged();
        }

        private void OnParameterPropertyChanged(object? sender, PropertyChangedEventArgs e) => RaiseCurrentEffectChanged();

        private static IReadOnlyList<string> GetSuggestions(EffectKind kind) => kind switch
        {
            EffectKind.Wave =>
            [
                KnownEffectParameters.WaveBands,
                KnownEffectParameters.WaveFrequency,
                KnownEffectParameters.WaveAmplitude
            ],
            EffectKind.MovingColors =>
            [
                KnownEffectParameters.MovingColorsDirectionX,
                KnownEffectParameters.MovingColorsDirectionY,
                KnownEffectParameters.MovingColorsWrap
            ],
            EffectKind.Shimmer =>
            [
                KnownEffectParameters.ShimmerAngleDegrees,
                KnownEffectParameters.ShimmerWidthRatio,
                KnownEffectParameters.ShimmerOpacityMultiplier
            ],
            EffectKind.Noise =>
            [
                KnownEffectParameters.NoiseOctaves,
                KnownEffectParameters.NoiseFrequency,
                KnownEffectParameters.NoiseContrast
            ],
            EffectKind.Lottie =>
            [
                LottieEffectParameters.AssetName,
                LottieEffectParameters.Url,
                LottieEffectParameters.Loop,
                LottieEffectParameters.AutoPlay,
                LottieEffectParameters.Speed,
                LottieEffectParameters.TintColorHex,
                LottieEffectParameters.ScaleMode
            ],
            _ => []
        };

        private static string GetDefaultValue(string key)
        {
            if (key.Equals(LottieEffectParameters.Loop, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(LottieEffectParameters.AutoPlay, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.MovingColorsWrap, StringComparison.OrdinalIgnoreCase)) return bool.TrueString;
            if (key.Equals(LottieEffectParameters.Speed, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.WaveFrequency, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.WaveAmplitude, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.MovingColorsDirectionX, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.MovingColorsDirectionY, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.ShimmerWidthRatio, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.ShimmerOpacityMultiplier, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.NoiseFrequency, StringComparison.OrdinalIgnoreCase) ||
                key.Equals(KnownEffectParameters.NoiseContrast, StringComparison.OrdinalIgnoreCase)) return "1";
            if (key.Equals(KnownEffectParameters.WaveBands, StringComparison.OrdinalIgnoreCase)) return "4";
            if (key.Equals(KnownEffectParameters.NoiseOctaves, StringComparison.OrdinalIgnoreCase)) return "3";
            if (key.Equals(KnownEffectParameters.ShimmerAngleDegrees, StringComparison.OrdinalIgnoreCase)) return "45";
            if (key.Equals(LottieEffectParameters.ScaleMode, StringComparison.OrdinalIgnoreCase)) return "AspectFit";

            return string.Empty;
        }
    }
}
