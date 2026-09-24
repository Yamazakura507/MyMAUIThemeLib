using CommunityToolkit.Mvvm.ComponentModel;
using ThemeForge.Abstractions.Records.UseOfGeometry;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel редактора геометрии.
    /// </summary>
    public partial class GeometryEditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private double cornerRadius = 8;

        [ObservableProperty]
        private double borderThickness = 1;

        [ObservableProperty]
        private double padding = 12;

        [ObservableProperty]
        private double margin = 8;

        [ObservableProperty]
        private double thumbDiameter = 16;

        [ObservableProperty]
        private double trackThickness = 4;

        [ObservableProperty]
        private double indicatorSize = 18;

        [ObservableProperty]
        private bool hasShadow;

        [ObservableProperty]
        private double shadowRadius = 4;

        [ObservableProperty]
        private double shadowOpacity = 0.18;

        [ObservableProperty]
        private double elevation;

        /// <summary>
        /// Строит настройки геометрии.
        /// </summary>
        public GeometrySettings BuildSettings() => new GeometrySettings
        {
            CornerRadius = CornerRadius,
            BorderThickness = BorderThickness,
            Padding = Padding,
            Margin = Margin,
            ThumbDiameter = ThumbDiameter,
            TrackThickness = TrackThickness,
            IndicatorSize = IndicatorSize,
            HasShadow = HasShadow,
            ShadowRadius = ShadowRadius,
            ShadowOpacity = ShadowOpacity,
            Elevation = Elevation
        };
    }
}
