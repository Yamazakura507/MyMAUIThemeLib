namespace Abstractions.Enums
{
    /// <summary>
    /// Состояния компонента интерфейса.
    /// Используется как флаги, но некоторые состояния взаимоисключающие на уровне логики UI.
    /// </summary>
    [Flags]
    public enum ComponentState
    {
        /// <summary>
        /// Обычное состояние.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Отключенное состояние.
        /// </summary>
        Disabled = 1 << 0,

        /// <summary>
        /// Выбранное состояние.
        /// </summary>
        Selected = 1 << 1,

        /// <summary>
        /// Отмеченное состояние для CheckBox/RadioButton/Switch-like контролов.
        /// </summary>
        Checked = 1 << 2,

        /// <summary>
        /// Неотмеченное состояние.
        /// </summary>
        Unchecked = 1 << 3,

        /// <summary>
        /// Наведение указателя.
        /// </summary>
        Hover = 1 << 4,

        /// <summary>
        /// Нажатие.
        /// </summary>
        Pressed = 1 << 5,

        /// <summary>
        /// Фокус клавиатуры/указателя.
        /// </summary>
        Focused = 1 << 6,

        /// <summary>
        /// Включенное состояние переключателя.
        /// </summary>
        On = 1 << 7,

        /// <summary>
        /// Выключенное состояние переключателя.
        /// </summary>
        Off = 1 << 8,

        /// <summary>
        /// Перетаскивание, например для Slider.
        /// </summary>
        Dragging = 1 << 9,

        /// <summary>
        /// Неопределенное состояние CheckBox.
        /// </summary>
        Indeterminate = 1 << 10
    }
}
