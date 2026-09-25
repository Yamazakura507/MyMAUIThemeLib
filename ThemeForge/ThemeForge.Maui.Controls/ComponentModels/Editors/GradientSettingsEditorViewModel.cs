using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ThemeForge.Abstractions.Enums;
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

        /// <summary>
        /// Доступные типы градиента.
        /// </summary>
        public Array Types { get; } = Enum.GetValues(typeof(GradientType));

        /// <summary>
        /// Редактор фона/эффекта градиента.
        /// </summary>
        public EffectSettingsEditorViewModel EffectEditor { get; } = new();

        /// <summary>
        /// Остановки градиента.
        /// </summary>
        public ObservableCollection<GradientStopViewModel> Stops { get; } = [];

        /// <summary>
        /// Создает ViewModel настроек градиента.
        /// </summary>
        public GradientSettingsEditorViewModel()
        {
            EnsureStops(ColorCount);
            EffectEditor.PropertyChanged += (_, __) => OnPropertyChanged(nameof(EffectEditor));
        }

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

            EffectSettings effect = EffectEditor.BuildEffectSettings();

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

            EffectEditor.LoadFrom(gradient?.BackgroundEffect);
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
