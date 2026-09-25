namespace ThemeForge.Maui.Studio.Enums
{
    /// <summary>
    /// Тип активного редактора в правой панели студии.
    /// </summary>
    public enum EditorKind
    {
        /// <summary>
        /// Редактор не выбран.
        /// </summary>
        None,

        /// <summary>
        /// Создание/редактирование простой цветовой темы.
        /// </summary>
        AddSolid,

        /// <summary>
        /// Создание/редактирование градиентной темы.
        /// </summary>
        AddGradient,

        /// <summary>
        /// Редактор типографики.
        /// </summary>
        Typography,

        /// <summary>
        /// Редактор геометрии.
        /// </summary>
        Geometry,

        /// <summary>
        /// Редактор цветов конкретного компонента.
        /// </summary>
        ComponentColors
    }
}
