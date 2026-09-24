using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Maui.Controls.ComponentModels.Trees;

namespace ThemeForge.Maui.Controls.Helpers
{
    /// <summary>
    /// Строит дерево готовых тем и кастомных настроек.
    /// </summary>
    public static class ThemeTreeBuilder
    {
        /// <summary>
        /// Строит корень дерева готовых тем.
        /// </summary>
        public static ThemeTreeNodeViewModel BuildReadyThemesRoot(IEnumerable<ThemeDefinition> themes)
        {
            ArgumentNullException.ThrowIfNull(themes);

            ThemeTreeNodeViewModel root = new ()
            {
                Title = "Готовые темы",
                Kind = TreeNodeKind.Root,
                IsExpanded = true
            };

            AddGroupedCategories(root, themes, t => t.Solid is not null, t => t.Solid!.Harmony, GetHarmonyTitle);
            AddGroupedCategories(root, themes, t => t.Gradient is not null, t => t.Gradient!.Type, GetGradientTitle);

            return root;
        }

        /// <summary>
        /// Строит корень дерева кастомных настроек.
        /// </summary>
        public static ThemeTreeNodeViewModel BuildCustomRoot(IControlThemeCatalog catalog)
        {
            ArgumentNullException.ThrowIfNull(catalog);

            ThemeTreeNodeViewModel root = new ()
            {
                Title = "Кастом",
                Kind = TreeNodeKind.Root,
                IsExpanded = true
            };

            foreach (ControlThemeDescriptor descriptor in catalog.GetDescriptors())
            {
                ThemeTreeNodeViewModel controlNode = new ()
                {
                    Title = descriptor.DisplayName,
                    Detail = descriptor.Description,
                    Icon = descriptor.Icon,
                    Kind = TreeNodeKind.Control,
                    Descriptor = descriptor,
                    Payload = descriptor,
                    IsExpanded = false
                };

                foreach (ComponentState state in descriptor.SupportedStates)
                {
                    ThemeTreeNodeViewModel stateNode = new ()
                    {
                        Title = GetStateTitle(state),
                        Kind = TreeNodeKind.State,
                        Descriptor = descriptor,
                        State = state,
                        Payload = state,
                        IsExpanded = false
                    };

                    foreach (ThemeSettingGroup group in descriptor.AllowedGroups)
                    {
                        stateNode.Children.Add(new ThemeTreeNodeViewModel
                        {
                            Title = GetGroupTitle(group),
                            Kind = TreeNodeKind.SettingGroup,
                            Descriptor = descriptor,
                            State = state,
                            Group = group,
                            Payload = group,
                            IsExpanded = false
                        });
                    }

                    controlNode.Children.Add(stateNode);
                }

                root.Children.Add(controlNode);
            }

            return root;
        }

        private static void AddGroupedCategories<TKey>(
                        ThemeTreeNodeViewModel root,
                        IEnumerable<ThemeDefinition> themes,
                        Func<ThemeDefinition, bool> filter,
                        Func<ThemeDefinition, TKey> keySelector,
                        Func<TKey, string> titleSelector)
        {
            IEnumerable<IGrouping<TKey, ThemeDefinition>> groups = themes.Where(filter).GroupBy(keySelector);

            foreach (IGrouping<TKey, ThemeDefinition> group in groups)
            {
                ThemeTreeNodeViewModel category = new()
                {
                    Title = titleSelector(group.Key),
                    Kind = TreeNodeKind.Category,
                    Payload = group.Key,
                    IsExpanded = false
                };

                foreach (ThemeDefinition theme in group)
                {
                    category.Children.Add(CreatePresetNode(theme));
                }

                root.Children.Add(category);
            }
        }

        private static ThemeTreeNodeViewModel CreatePresetNode(ThemeDefinition theme) => new ThemeTreeNodeViewModel
        {
            Title = theme.Name,
            Detail = theme.Kind.ToString(),
            Kind = TreeNodeKind.Preset,
            Theme = theme,
            Payload = theme,
            IsExpanded = false
        };

        private static string GetHarmonyTitle(ColorHarmonyMode mode) => mode switch
        {
            ColorHarmonyMode.Monochrome => "Monochrome",
            ColorHarmonyMode.Analogous => "Analogous",
            ColorHarmonyMode.Complementary => "Complementary",
            ColorHarmonyMode.Triadic => "Triadic",
            ColorHarmonyMode.Tetradic => "Quad",
            _ => mode.ToString()
        };

        private static string GetGradientTitle(GradientType type) =>  type switch
        {
            GradientType.Linear => "Linear Gradient",
            GradientType.Radial => "Radial Gradient",
            _ => type.ToString()
        };

        private static string GetStateTitle(ComponentState state) => state switch
        {
            ComponentState.Default => "Default",
            ComponentState.Disabled => "Disabled",
            ComponentState.Selected => "Selected",
            ComponentState.Checked => "Checked",
            ComponentState.Unchecked => "Unchecked",
            ComponentState.Hover => "Hover",
            ComponentState.Pressed => "Pressed",
            ComponentState.Focused => "Focused",
            ComponentState.On => "On",
            ComponentState.Off => "Off",
            ComponentState.Dragging => "Dragging",
            ComponentState.Indeterminate => "Indeterminate",
            _ => state.ToString()
        };

        private static string GetGroupTitle(ThemeSettingGroup group) => group switch
        {
            ThemeSettingGroup.Colors => "Цвета",
            ThemeSettingGroup.Typography => "Текст",
            ThemeSettingGroup.Geometry => "Геометрия",
            ThemeSettingGroup.Effects => "Эффекты",
            ThemeSettingGroup.Layout => "Раскладка",
            ThemeSettingGroup.Interaction => "Взаимодействие",
            ThemeSettingGroup.Content => "Контент",
            _ => group.ToString()
        };
    }
}
