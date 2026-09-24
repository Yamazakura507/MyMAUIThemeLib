using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ThemeForge.Maui.Controls.ComponentModels.Trees
{
    /// <summary>
    /// ViewModel универсального дерева тем.
    /// </summary>
    public partial class ThemeTreeViewModel : ObservableObject
    {
        /// <summary>
        /// Корневые узлы дерева.
        /// </summary>
        public ObservableCollection<ThemeTreeNodeViewModel> Roots { get; } = [];

        /// <summary>
        /// Плоский список видимых узлов для CollectionView.
        /// </summary>
        public ObservableCollection<ThemeTreeNodeViewModel> VisibleNodes { get; } = [];

        /// <summary>
        /// Устанавливает корни дерева и перестраивает видимые узлы.
        /// </summary>
        public void SetRoots(IEnumerable<ThemeTreeNodeViewModel> roots)
        {
            ArgumentNullException.ThrowIfNull(roots);

            Unsubscribe(Roots);

            Roots.Clear();

            foreach (ThemeTreeNodeViewModel root in roots)
            {
                Roots.Add(root);
            }

            Subscribe(Roots);
            RebuildVisible();
        }

        /// <summary>
        /// Перестраивает плоский список видимых узлов.
        /// </summary>
        [RelayCommand]
        private void RebuildVisible()
        {
            IReadOnlyList<ThemeTreeNodeViewModel> flattened = ThemeTreeFlattener.Flatten(Roots);

            VisibleNodes.Clear();

            foreach (ThemeTreeNodeViewModel node in flattened)
            {
                VisibleNodes.Add(node);
            }
        }

        private void Subscribe(IEnumerable<ThemeTreeNodeViewModel> nodes)
        {
            foreach (ThemeTreeNodeViewModel node in nodes)
            {
                node.PropertyChanged += OnNodePropertyChanged;

                Subscribe(node.Children);
            }
        }

        private void Unsubscribe(IEnumerable<ThemeTreeNodeViewModel> nodes)
        {
            foreach (ThemeTreeNodeViewModel node in nodes)
            {
                node.PropertyChanged -= OnNodePropertyChanged;

                Unsubscribe(node.Children);
            }
        }

        private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeTreeNodeViewModel.IsExpanded))
            {
                RebuildVisible();
            }
        }
    }
}
