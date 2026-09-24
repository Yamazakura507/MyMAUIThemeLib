namespace ThemeForge.Abstractions.Enums
{
    /// <summary>
    /// Тип узла универсального дерева.
    /// </summary>
    public enum TreeNodeKind
    {
        /// <summary>
        /// Корневой узел.
        /// </summary>
        Root,

        /// <summary>
        /// Категория/группа.
        /// </summary>
        Category,

        /// <summary>
        /// Готовая тема/пресет.
        /// </summary>
        Preset,

        /// <summary>
        /// Группа настроек: Colors, Typography, Geometry и т.д.
        /// </summary>
        SettingGroup,

        /// <summary>
        /// Конкретный контрол.
        /// </summary>
        Control,

        /// <summary>
        /// Состояние контрола.
        /// </summary>
        State,

        /// <summary>
        /// Отдельное свойство/настройка.
        /// </summary>
        Property,

        /// <summary>
        /// Действие: добавить, удалить, применить и т.п.
        /// </summary>
        Action,

        /// <summary>
        /// Узел добавления нового элемента.
        /// </summary>
        Add
    }
}
