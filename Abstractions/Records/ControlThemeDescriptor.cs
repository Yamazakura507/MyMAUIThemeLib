using Abstractions.Enums;

namespace Abstractions.Records
{
    /// <summary>
    /// Описание настраиваемого контрола.
    /// </summary>
    /// <param name="ControlType">Технический тип контрола.</param>
    /// <param name="DisplayName">Отображаемое имя.</param>
    public sealed record ControlThemeDescriptor(string ControlType, string DisplayName)
    {
        /// <summary>
        /// Иконка контрола.
        /// </summary>
        public string? Icon { get; init; }

        /// <summary>
        /// Описание контрола.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Поддерживаемые состояния.
        /// </summary>
        public IReadOnlyList<ComponentState> SupportedStates { get; init; } = new[] { ComponentState.Default };

        /// <summary>
        /// Разрешенные группы настроек.
        /// </summary>
        public IReadOnlyList<ThemeSettingGroup> AllowedGroups { get; init; } = new[]
        {
            ThemeSettingGroup.Colors,
            ThemeSettingGroup.Typography,
            ThemeSettingGroup.Geometry
        };

        /// <summary>
        /// Поддерживает ли контрол состояния On/Off.
        /// </summary>
        public bool SupportsOnOff { get; init; }

        /// <summary>
        /// Поддерживает ли контрол состояния Checked/Unchecked.
        /// </summary>
        public bool SupportsCheckedUnchecked { get; init; }

        /// <summary>
        /// Поддерживает ли контрол неопределенное состояние.
        /// </summary>
        public bool SupportsIndeterminate { get; init; }

        /// <summary>
        /// Можно ли применять градиент к фону этого контрола.
        /// </summary>
        public bool SupportsGradientBackground { get; init; } = true;
    }

}
