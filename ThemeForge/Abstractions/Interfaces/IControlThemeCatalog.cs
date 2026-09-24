using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records;

namespace ThemeForge.Abstractions.Interfaces
{
    /// <summary>
    /// Каталог контролов, доступных в кастомном режиме.
    /// </summary>
    public interface IControlThemeCatalog
    {
        /// <summary>
        /// Возвращает все описания контролов.
        /// </summary>
        IReadOnlyList<ControlThemeDescriptor> GetDescriptors();

        /// <summary>
        /// Возвращает описание конкретного контрола.
        /// </summary>
        ControlThemeDescriptor? GetDescriptor(string controlType);

        /// <summary>
        /// Проверяет, разрешена ли группа настроек для контрола в определенном состоянии.
        /// </summary>
        bool IsGroupAllowed(string controlType, ThemeSettingGroup group, ComponentState state);
    }
}
