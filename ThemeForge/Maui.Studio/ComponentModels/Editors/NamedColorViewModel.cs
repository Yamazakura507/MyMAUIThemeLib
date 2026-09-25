using CommunityToolkit.Mvvm.ComponentModel;

namespace ThemeForge.Maui.Studio.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel одной именованной цветовой роли компонента.
    /// </summary>
    public partial class NamedColorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string key = string.Empty;

        [ObservableProperty]
        private string hex = "#FFFFFF";
    }
}
