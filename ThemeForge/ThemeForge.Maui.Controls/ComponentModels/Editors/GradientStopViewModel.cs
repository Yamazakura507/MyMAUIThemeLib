using CommunityToolkit.Mvvm.ComponentModel;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel одной остановки градиента.
    /// </summary>
    public partial class GradientStopViewModel : ObservableObject
    {
        [ObservableProperty]
        private string hex = "#000000";

        [ObservableProperty]
        private double offset;

        [ObservableProperty]
        private string? name;
    }
}
