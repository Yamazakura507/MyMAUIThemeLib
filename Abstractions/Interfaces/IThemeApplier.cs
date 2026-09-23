using Abstractions.Records.UseOfTheme;

namespace Abstractions.Interfaces
{
    /// <summary>
    /// Применитель темы.
    /// </summary>
    public interface IThemeApplier
    {
        /// <summary>
        /// Применяет тему к ресурсам приложения/окна/страницы.
        /// </summary>
        void Apply(ThemeDefinition theme);

        /// <summary>
        /// Сбрасывает примененные ресурсы к значениям по умолчанию.
        /// </summary>
        void Reset();
    }
}
