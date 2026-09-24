
using ThemeForge.Abstractions.Enums;

namespace ThemeForge.Maui.Controls.ComponentModels.Trees
{
    /// <summary>
    /// Выбирает шаблон отображения узла дерева в зависимости от его типа.
    /// </summary>
    public sealed class ThemeTreeNodeTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Шаблон корневого узла.
        /// </summary>
        public DataTemplate? RootTemplate { get; set; }

        /// <summary>
        /// Шаблон категории.
        /// </summary>
        public DataTemplate? CategoryTemplate { get; set; }

        /// <summary>
        /// Шаблон готовой темы/пресета.
        /// </summary>
        public DataTemplate? PresetTemplate { get; set; }

        /// <summary>
        /// Шаблон контрола в кастомном режиме.
        /// </summary>
        public DataTemplate? ControlTemplate { get; set; }

        /// <summary>
        /// Шаблон состояния контрола.
        /// </summary>
        public DataTemplate? StateTemplate { get; set; }

        /// <summary>
        /// Шаблон группы настроек.
        /// </summary>
        public DataTemplate? SettingGroupTemplate { get; set; }

        /// <summary>
        /// Шаблон конкретного свойства.
        /// </summary>
        public DataTemplate? PropertyTemplate { get; set; }

        /// <summary>
        /// Шаблон действия.
        /// </summary>
        public DataTemplate? ActionTemplate { get; set; }

        /// <summary>
        /// Шаблон узла добавления.
        /// </summary>
        public DataTemplate? AddTemplate { get; set; }

        /// <inheritdoc />
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is not ThemeTreeNodeViewModel node)
            {
                return PropertyTemplate ?? CategoryTemplate ?? throw new InvalidOperationException("Нет шаблона по умолчанию.");
            }

            return node.Kind switch
            {
                TreeNodeKind.Root => RootTemplate ?? CategoryTemplate ?? PropertyTemplate!,
                TreeNodeKind.Category => CategoryTemplate ?? PropertyTemplate!,
                TreeNodeKind.Preset => PresetTemplate ?? CategoryTemplate ?? PropertyTemplate!,
                TreeNodeKind.Control => ControlTemplate ?? CategoryTemplate ?? PropertyTemplate!,
                TreeNodeKind.State => StateTemplate ?? CategoryTemplate ?? PropertyTemplate!,
                TreeNodeKind.SettingGroup => SettingGroupTemplate ?? CategoryTemplate ?? PropertyTemplate!,
                TreeNodeKind.Property => PropertyTemplate ?? CategoryTemplate!,
                TreeNodeKind.Action => ActionTemplate ?? PropertyTemplate!,
                TreeNodeKind.Add => AddTemplate ?? ActionTemplate ?? PropertyTemplate!,
                _ => PropertyTemplate ?? CategoryTemplate!
            };
        }
    }
}
