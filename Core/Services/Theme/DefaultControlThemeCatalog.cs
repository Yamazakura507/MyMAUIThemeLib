using Abstractions.Enums;
using Abstractions.Interfaces;
using Abstractions.Knowledge;
using Abstractions.Records;

namespace Core.Services.Theme
{
    /// <summary>
    /// Базовый каталог настраиваемых контролов.
    /// </summary>
    public sealed class DefaultControlThemeCatalog : IControlThemeCatalog
    {
        private static readonly ThemeSettingGroup[] VisualGroups =
        [
            ThemeSettingGroup.Colors,
            ThemeSettingGroup.Typography,
            ThemeSettingGroup.Geometry,
            ThemeSettingGroup.Effects
        ];

        private static readonly ThemeSettingGroup[] ContainerGroups =
        [
            ThemeSettingGroup.Colors,
            ThemeSettingGroup.Geometry,
            ThemeSettingGroup.Effects,
            ThemeSettingGroup.Layout
        ];

        private static readonly ThemeSettingGroup[] TextGroups =
        [
            ThemeSettingGroup.Colors,
            ThemeSettingGroup.Typography,
            ThemeSettingGroup.Geometry
        ];

        private readonly Dictionary<string, ControlThemeDescriptor> map;
        private readonly IReadOnlyList<ControlThemeDescriptor> descriptors;

        /// <summary>
        /// Создает каталог по умолчанию.
        /// </summary>
        public DefaultControlThemeCatalog()
        {
            descriptors =
            [
                Create(KnownControlTypes.Button, "Button", "Основная кнопка.",
                [ComponentState.Default, ComponentState.Disabled, ComponentState.Hover, ComponentState.Pressed, ComponentState.Focused, ComponentState.Selected],
                VisualGroups),
                Create(KnownControlTypes.Label, "Label", "Текстовая метка.", [ComponentState.Default, ComponentState.Disabled], TextGroups),
                Create(KnownControlTypes.Entry, "Entry", "Однострочное поле ввода.",
                    [ComponentState.Default, ComponentState.Disabled, ComponentState.Focused],
                    VisualGroups),
                Create(KnownControlTypes.Editor, "Editor", "Многострочное поле ввода.", [ComponentState.Default, ComponentState.Disabled, ComponentState.Focused], VisualGroups),
                Create(KnownControlTypes.CheckBox, "CheckBox", "Флажок.",
                    [ComponentState.Default, ComponentState.Disabled, ComponentState.Checked, ComponentState.Unchecked, ComponentState.Indeterminate],
                    VisualGroups,
                    supportsCheckedUnchecked: true,
                    supportsIndeterminate: true),
                Create(KnownControlTypes.RadioButton, "RadioButton", "Переключатель.",
                    [ComponentState.Default, ComponentState.Disabled, ComponentState.Checked, ComponentState.Unchecked],
                    VisualGroups,
                    supportsCheckedUnchecked: true),
                Create(KnownControlTypes.Switch, "Switch", "Переключатель On/Off.",
                    [ComponentState.Default, ComponentState.Disabled, ComponentState.On, ComponentState.Off],
                    VisualGroups,
                    supportsOnOff: true),
                Create(KnownControlTypes.Slider, "Slider", "Ползунок.", [ComponentState.Default, ComponentState.Disabled, ComponentState.Dragging], VisualGroups),
                Create(KnownControlTypes.ProgressBar, "ProgressBar", "Индикатор прогресса.", [ComponentState.Default, ComponentState.Disabled], VisualGroups),
                Create(KnownControlTypes.Picker, "Picker", "Выпадающий список выбора.", [ComponentState.Default, ComponentState.Disabled, ComponentState.Focused], VisualGroups),
                Create(KnownControlTypes.DatePicker, "DatePicker", "Выбор даты.", [ComponentState.Default, ComponentState.Disabled, ComponentState.Focused], VisualGroups),
                Create(KnownControlTypes.TimePicker, "TimePicker", "Выбор времени.", [ComponentState.Default, ComponentState.Disabled, ComponentState.Focused], VisualGroups),
                Create(KnownControlTypes.SearchBar, "SearchBar", "Строка поиска.", [ComponentState.Default, ComponentState.Disabled, ComponentState.Focused], VisualGroups),
                Create(KnownControlTypes.Tab, "Tab", "Вкладка.",
                    [ComponentState.Default, ComponentState.Disabled, ComponentState.Selected, ComponentState.Hover],
                    VisualGroups),
                Create(KnownControlTypes.TabBar, "TabBar", "Панель вкладок.", [ComponentState.Default, ComponentState.Disabled], ContainerGroups),
                Create(KnownControlTypes.Flyout, "Flyout", "Боковое меню.", [ComponentState.Default, ComponentState.Disabled, ComponentState.Selected], ContainerGroups),
                Create(KnownControlTypes.Tree, "Tree", "Дерево элементов.",
                    [ComponentState.Default, ComponentState.Disabled, ComponentState.Selected, ComponentState.Hover],
                    ContainerGroups),
                Create(KnownControlTypes.List, "List", "Список элементов.",
                    [ComponentState.Default, ComponentState.Disabled, ComponentState.Selected, ComponentState.Hover],
                    ContainerGroups),
                Create(KnownControlTypes.Card, "Card", "Карточка.", [ComponentState.Default, ComponentState.Disabled, ComponentState.Selected], ContainerGroups),
                Create(KnownControlTypes.Badge, "Badge", "Значок/бейдж.", [ComponentState.Default, ComponentState.Disabled], VisualGroups),
                Create(KnownControlTypes.Avatar, "Avatar", "Аватар.", [ComponentState.Default, ComponentState.Disabled], VisualGroups),
                Create(KnownControlTypes.Tooltip, "Tooltip", "Подсказка.", [ComponentState.Default], ContainerGroups),
                Create(KnownControlTypes.Dialog, "Dialog", "Диалог.", [ComponentState.Default], ContainerGroups),
                Create(KnownControlTypes.Snackbar, "Snackbar", "Уведомление.", [ComponentState.Default], ContainerGroups)
            ];
            map = descriptors.ToDictionary(d => d.ControlType, d => d, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public IReadOnlyList<ControlThemeDescriptor> GetDescriptors() => descriptors;

        /// <inheritdoc />
        public ControlThemeDescriptor? GetDescriptor(string controlType)
        {
            if (string.IsNullOrWhiteSpace(controlType)) return null;

            return map.TryGetValue(controlType, out ControlThemeDescriptor? descriptor) ? descriptor : null;
        }

        /// <inheritdoc />
        public bool IsGroupAllowed(string controlType, ThemeSettingGroup group, ComponentState state)
        {
            ControlThemeDescriptor? descriptor = GetDescriptor(controlType);

            return descriptor?.AllowedGroups.Contains(group) == true;
        }

        private static ControlThemeDescriptor Create(
            string controlType,
            string displayName,
            string description,
            ComponentState[] states,
            ThemeSettingGroup[] groups,
            bool supportsOnOff = false,
            bool supportsCheckedUnchecked = false,
            bool supportsIndeterminate = false)
        {
            return new ControlThemeDescriptor(controlType, displayName)
            {
                Description = description,
                SupportedStates = states,
                AllowedGroups = groups,
                SupportsOnOff = supportsOnOff,
                SupportsCheckedUnchecked = supportsCheckedUnchecked,
                SupportsIndeterminate = supportsIndeterminate
            };
        }
    }
}
