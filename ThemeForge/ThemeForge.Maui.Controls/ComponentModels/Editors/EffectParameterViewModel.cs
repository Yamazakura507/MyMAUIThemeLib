using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace ThemeForge.Maui.Controls.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel одного параметра эффекта.
    /// </summary>
    public partial class EffectParameterViewModel : ObservableObject
    {
        [ObservableProperty]
        private string key = string.Empty;

        [ObservableProperty]
        private string value = string.Empty;

        /// <summary>
        /// Команда удаления этого параметра из родительского списка.
        /// </summary>
        public ICommand RemoveCommand { get; set; } = new Command(() => { });
    }
}
