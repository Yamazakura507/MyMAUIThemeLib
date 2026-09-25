using ThemeForge.Maui.Controls.ComponentModels.Trees;

namespace ThemeForge.Maui.Controls.Components.Tree;

/// <summary>
/// Универсальный многоуровневый контрол дерева.
/// </summary>
public partial class ThemeTreeView : ContentView
{
    /// <summary>
    /// Bindable-свойство выбранного узла.
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
                                                                            nameof(SelectedItem),
                                                                            typeof(ThemeTreeNodeViewModel),
                                                                            typeof(ThemeTreeView),
                                                                            null,
                                                                            BindingMode.TwoWay);

    /// <summary>
    /// Создает контрол дерева.
    /// </summary>
    public ThemeTreeView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Текущий выбранный узел дерева.
    /// </summary>
    public ThemeTreeNodeViewModel? SelectedItem
    {
        get => (ThemeTreeNodeViewModel?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
}