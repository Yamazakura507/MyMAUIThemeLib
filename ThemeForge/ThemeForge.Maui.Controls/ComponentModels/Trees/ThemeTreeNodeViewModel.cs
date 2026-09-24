using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records;
using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Maui.Controls.ComponentModels.Trees
{
    /// <summary>
    /// Observable-представление узла универсального дерева тем.
    /// </summary>
    public partial class ThemeTreeNodeViewModel : ObservableObject, IThemeTreeNode
    {
        [ObservableProperty]
        private bool isExpanded;

        [ObservableProperty]
        private int depth;

        /// <inheritdoc />
        public string Title { get; init; } = string.Empty;

        /// <inheritdoc />
        public string? Detail { get; init; }

        /// <inheritdoc />
        public string? Icon { get; init; }

        /// <inheritdoc />
        public TreeNodeKind Kind { get; init; }

        /// <inheritdoc />
        public object? Payload { get; init; }

        /// <summary>
        /// Тема, если узел представляет готовую/пользовательскую тему.
        /// </summary>
        public ThemeDefinition? Theme { get; init; }

        /// <summary>
        /// Дескриптор контрола, если узел относится к кастомному режиму.
        /// </summary>
        public ControlThemeDescriptor? Descriptor { get; init; }

        /// <summary>
        /// Состояние контрола, если узел относится к состоянию.
        /// </summary>
        public ComponentState State { get; init; } = ComponentState.Default;

        /// <summary>
        /// Группа настроек, если узел относится к настройкам.
        /// </summary>
        public ThemeSettingGroup? Group { get; init; }

        /// <summary>
        /// Дочерние узлы.
        /// </summary>
        public ObservableCollection<ThemeTreeNodeViewModel> Children { get; } = [];

        /// <inheritdoc />
        IReadOnlyList<IThemeTreeNode> IThemeTreeNode.Children => Children;

        /// <summary>
        /// Есть ли у узла дочерние элементы.
        /// </summary>
        public bool HasChildren => Children.Count > 0;

        /// <summary>
        /// Переключает раскрытие узла.
        /// </summary>
        [RelayCommand]
        private void Toggle()
        {
            IsExpanded = !IsExpanded;
        }
    }
}
