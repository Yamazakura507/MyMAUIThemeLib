using ThemeForge.Abstractions.Records.UseOfTheme;

namespace ThemeForge.Abstractions.Interfaces
{
    /// <summary>
    /// Сервис формирования человекочитаемых имен тем.
    /// </summary>
    public interface IThemeNamingService
    {
        /// <summary>
        /// Строит отображаемое имя темы согласно правилам:
        /// Solid: Режим - Название цвета.
        /// Gradient: Тип градиента - Количество цветов - Цвета.
        /// </summary>
        string BuildDisplayName(ThemeDefinition theme);

        /// <summary>
        /// Строит короткое имя для элемента списка/дерева.
        /// </summary>
        string BuildPresetTitle(ThemeDefinition theme);

        /// <summary>
        /// Строит подпись/описание для элемента списка/дерева.
        /// </summary>
        string BuildPresetDetail(ThemeDefinition theme);
    }
}
