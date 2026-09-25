using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Helpers;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfColors.Gradients;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel редактора градиентной темы.
    /// </summary>
    public partial class GradientThemeEditorViewModel : ObservableObject
    {
        private static readonly string[] DefaultColors =
        [
            "#1976D2",
            "#7E57C2",
            "#EC407A",
            "#FFC107"
        ];

        private readonly IThemeFactory themeFactory;

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
        private ThemeDefinition? previewTheme;

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
        /// Создает ViewModel редактора градиента.
        /// </summary>
        public GradientThemeEditorViewModel(IThemeFactory themeFactory)
        {
            this.themeFactory = themeFactory;
            EnsureStops(ColorCount);
        }

        partial void OnColorCountChanged(int value) => EnsureStops(value);

        /// <summary>
        /// Собирает тему из текущих настроек.
        /// </summary>
        [RelayCommand]
        private async Task BuildAsync(CancellationToken cancellationToken)
        {
            List<string> hexes = Stops.Select(s => s.Hex).ToList();

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

            PreviewTheme = await themeFactory.CreateGradientAsync(Type, hexes, geometry, effect, cancellationToken: cancellationToken);
        }

        private IReadOnlyDictionary<string, string> BuildEffectParameters()
        {
            if (!EffectEnabled || EffectKind != EffectKind.Lottie)
            {
                return new Dictionary<string, string>();
            }

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
                int index = Stops.Count;

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
    }
}
