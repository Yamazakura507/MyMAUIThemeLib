using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Knowledge;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Core.Helpers;
using ThemeForge.Maui.Studio.Helpers;

namespace ThemeForge.Maui.Studio.ComponentModels.Editors
{
    /// <summary>
    /// ViewModel редактора цветов конкретного компонента в конкретном состоянии.
    /// </summary>
    public partial class ComponentThemeEditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = "Компонент";

        [ObservableProperty]
        private string? controlType;

        [ObservableProperty]
        private ComponentState state = ComponentState.Default;

        [ObservableProperty]
        private string foregroundHex = "#1F1F1F";

        [ObservableProperty]
        private string backgroundHex = "#FFFFFF";

        [ObservableProperty]
        private string borderHex = "#DADCE0";

        /// <summary>
        /// Загружает редактор из темы.
        /// </summary>
        public void LoadFrom(ThemeDefinition theme,string controlTypeName, ComponentState componentState)
        {
            ArgumentNullException.ThrowIfNull(theme);
            ArgumentException.ThrowIfNullOrWhiteSpace(controlTypeName);

            ControlType = controlTypeName;
            State = componentState;
            Title = $"{controlTypeName} · {componentState}";

            var component = theme.GetComponentTheme(controlTypeName, componentState);

            ForegroundHex = component.Foreground?.Hex ?? "#1F1F1F";
            BackgroundHex = component.Background?.Hex ?? "#FFFFFF";
            BorderHex = component.Border?.Hex ?? "#DADCE0";
        }

        /// <summary>
        /// Строит модель компонента из текущих значений.
        /// </summary>
        public ComponentTheme BuildComponentTheme()
        {
            string control = ControlType ?? KnownControlTypes.Button;

            return new ComponentTheme(control)
            {
                State = State,
                Foreground = CreateToken(ForegroundHex),
                Background = CreateToken(BackgroundHex),
                Border = CreateToken(BorderHex)
            };
        }

        private static ColorToken? CreateToken(string hex)
        {
            if (!ColorUtility.TryNormalizeHex(hex, out var normalized)) return null;

            return new ColorToken
            {
                Hex = normalized,
                Source = ColorNameSource.User
            };
        }
    }
}
