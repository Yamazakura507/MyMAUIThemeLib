using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Helpers;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfColors.Gradients;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Abstractions.Records.UseOfTheme;
using GradientStop = ThemeForge.Abstractions.Records.UseOfColors.Gradients.GradientStop;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// Переиспользуемая ViewModel для редактирования настроек градиента.
    /// </summary>
    public partial class GradientSettingsEditorViewModel : ObservableObject
    {
        private static readonly string[] DefaultColors =
        [
            "#1976D2",
            "#7E57C2",
            "#EC407A",
            "#FFC107"
        ];

        [ObservableProperty]
        private GradientType type = GradientType.Linear;

        [ObservableProperty]
        private int colorCount = 3;

        [ObservableProperty]
        private double angleDegrees = 90;

        [ObservableProperty]
        private double centerX = 0.5;

        [ObservableProperty]
        private double centerY = 0.5;

        [ObservableProperty]
        private double radius = 0.75;

        [ObservableProperty]
        private bool effectEnabled;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLottieEffect))]
        private EffectKind effectKind = EffectKind.MovingColors;

        [ObservableProperty]
        private double effectIntensity = 0.5;

        [ObservableProperty]
        private double effectSpeed = 1.0;

        [ObservableProperty]
        private string? lottieAssetName;

        [ObservableProperty]
        private string? lottieUrl;

        [ObservableProperty]
        private bool lottieLoop = true;

        [ObservableProperty]
        private bool lottieAutoPlay = true;

        [ObservableProperty]
        private string? lottieTintColorHex;

        [ObservableProperty]
        private string lottieScaleMode = "AspectFit";

        /// <summary>
        /// Доступные типы градиента.
        /// </summary>
        public Array Types { get; } = Enum.GetValues(typeof(GradientType));

        /// <summary>
        /// Доступные эффекты.
        /// </summary>
        public Array EffectKinds { get; } = Enum.GetValues(typeof(EffectKind));

        /// <summary>
        /// Доступные режимы масштабирования Lottie.
        /// </summary>
        public IReadOnlyList<string> LottieScaleModes { get; } =
        [
            "AspectFit",
            "AspectFill",
            "Zoom",
            "Uniform"
        ];

        /// <summary>
        /// True, если выбран Lottie-эффект.
        /// </summary>
        public bool IsLottieEffect => EffectKind == EffectKind.Lottie;

        /// <summary>
        /// Остановки градиента.
        /// </summary>
        public ObservableCollection<GradientStopViewModel> Stops { get; } = [];

        /// <summary>
        /// Создает ViewModel настроек градиента.
        /// </summary>
        public GradientSettingsEditorViewModel() => EnsureStops(ColorCount);

        partial void OnColorCountChanged(int value) => EnsureStops(value);

        /// <summary>
        /// Строит модель <see cref="GradientTheme"/> из текущих настроек.
        /// </summary>
        public GradientTheme BuildGradientTheme()
        {
            List<GradientStop> stops = Stops
                .Where(s => !string.IsNullOrWhiteSpace(s.Hex))
                .Select((s, index) => new GradientStop(
                    ColorToken.FromHex(s.Hex),
                    Stops.Count <= 1 ? 0 : (double)index / (Stops.Count - 1)))
                .ToList();

            if (stops.Count < 2)
            {
                stops =
                [
                    new GradientStop(ColorToken.FromHex("#1976D2"), 0),
                    new GradientStop(ColorToken.FromHex("#7E57C2"), 1)
                ];
            }

            GradientGeometry geometry = Type switch
            {
                GradientType.Linear => new GradientGeometry
                {
                    AngleDegrees = AngleDegrees,
                    StartPoint = new NormalizedPoint(0, 0.5),
                    EndPoint = new NormalizedPoint(1, 0.5)
                },
                GradientType.Radial => new GradientGeometry
                {
                    CenterPoint = new NormalizedPoint(CenterX, CenterY),
                    Radius = Radius
                },
                _ => new GradientGeometry()
            };

            EffectSettings? effect = EffectEnabled
                ? new EffectSettings
                {
                    IsEnabled = true,
                    Kind = EffectKind,
                    Intensity = EffectIntensity,
                    Speed = EffectSpeed,
                    Parameters = BuildEffectParameters()
                } : null;

            return new GradientTheme(Type, stops)
            {
                Geometry = geometry,
                BackgroundEffect = effect
            };
        }

        /// <summary>
        /// Загружает настройки из существующего градиента.
        /// </summary>
        public void LoadFrom(GradientTheme? gradient)
        {
            Type = gradient?.Type ?? GradientType.Linear;

            IReadOnlyList<GradientStop> stops = gradient?.Stops ?? [];
            ColorCount = Math.Clamp(stops.Count == 0 ? 3 : stops.Count, 2, 4);

            EnsureStops(ColorCount);

            if (stops.Count > 0)
            {
                for (int i = 0; i < Stops.Count && i < stops.Count; i++)
                {
                    Stops[i].Hex = stops[i].Color.Hex;
                    Stops[i].Offset = stops[i].Offset;
                    Stops[i].Name = stops[i].Color.Name;
                }
            }

            AngleDegrees = gradient?.Geometry.AngleDegrees ?? 90;
            CenterX = gradient?.Geometry.CenterPoint?.X ?? 0.5;
            CenterY = gradient?.Geometry.CenterPoint?.Y ?? 0.5;
            Radius = gradient?.Geometry.Radius ?? 0.75;

            EffectSettings? effect = gradient?.BackgroundEffect;
            EffectEnabled = effect?.IsEnabled ?? false;
            EffectKind = effect?.Kind ?? EffectKind.MovingColors;
            EffectIntensity = effect?.Intensity ?? 0.5;
            EffectSpeed = effect?.Speed ?? 1.0;

            if (effect is not null)
            {
                LottieAssetName = GetParameter(effect.Parameters, LottieEffectParameters.AssetName);
                LottieUrl = GetParameter(effect.Parameters, LottieEffectParameters.Url);
                LottieLoop = GetBoolParameter(effect.Parameters, LottieEffectParameters.Loop, true);
                LottieAutoPlay = GetBoolParameter(effect.Parameters, LottieEffectParameters.AutoPlay, true);
                LottieTintColorHex = GetParameter(effect.Parameters, LottieEffectParameters.TintColorHex);
                LottieScaleMode = GetParameter(effect.Parameters, LottieEffectParameters.ScaleMode) ?? "AspectFit";
            }
        }

        private IReadOnlyDictionary<string, string> BuildEffectParameters()
        {
            if (!EffectEnabled || EffectKind != EffectKind.Lottie) return new Dictionary<string, string>();

            Dictionary<string, string> parameters = new (StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(LottieAssetName))
            {
                parameters[LottieEffectParameters.AssetName] = LottieAssetName;
            }

            if (!string.IsNullOrWhiteSpace(LottieUrl))
            {
                parameters[LottieEffectParameters.Url] = LottieUrl;
            }

            parameters[LottieEffectParameters.Loop] = LottieLoop ? bool.TrueString : bool.FalseString;
            parameters[LottieEffectParameters.AutoPlay] = LottieAutoPlay ? bool.TrueString : bool.FalseString;
            parameters[LottieEffectParameters.Speed] = EffectSpeed.ToString("0.###", CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(LottieTintColorHex))
            {
                parameters[LottieEffectParameters.TintColorHex] = LottieTintColorHex;
            }

            if (!string.IsNullOrWhiteSpace(LottieScaleMode))
            {
                parameters[LottieEffectParameters.ScaleMode] = LottieScaleMode;
            }

            return parameters;
        }

        private void EnsureStops(int requestedCount)
        {
            int count = Math.Clamp(requestedCount, 2, 4);

            if (ColorCount != count)
            {
                ColorCount = count;
                return;
            }

            while (Stops.Count < count)
            {
                var index = Stops.Count;

                Stops.Add(new GradientStopViewModel
                {
                    Hex = DefaultColors[index % DefaultColors.Length],
                    Offset = count == 1 ? 0 : (double)index / (count - 1)
                });
            }

            while (Stops.Count > count)
            {
                Stops.RemoveAt(Stops.Count - 1);
            }

            RebalanceOffsets();
        }

        private void RebalanceOffsets()
        {
            if (Stops.Count <= 1) return;

            for (int i = 0; i < Stops.Count; i++)
            {
                Stops[i].Offset = (double)i / (Stops.Count - 1);
            }
        }

        private static string? GetParameter(IReadOnlyDictionary<string, string> parameters, string key)
        {
            return parameters.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
        }

        private static bool GetBoolParameter(IReadOnlyDictionary<string, string> parameters,string key,bool defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
        }
    }
}
