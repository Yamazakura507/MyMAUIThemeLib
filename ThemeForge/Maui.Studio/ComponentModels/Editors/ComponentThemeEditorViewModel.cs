using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Knowledge;
using ThemeForge.Abstractions.Records;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Core.Helpers;
using ThemeForge.Maui.Controls.ComponentModels.Editors;
using ThemeForge.Maui.Studio.Helpers;

namespace ThemeForge.Maui.Studio.ComponentModels.Editors
{
    /// <summary>
    /// Расширенная ViewModel редактора одного компонента в одном состоянии.
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

        [ObservableProperty]
        private bool useGradientBackground;

        [ObservableProperty]
        private bool supportsGradientBackground = true;

        /// <summary>
        /// Вложенный редактор градиента для фона компонента.
        /// </summary>
        public GradientSettingsEditorViewModel GradientSettings { get; } = new();

        /// <summary>
        /// Именованные цвета компонента.
        /// </summary>
        public ObservableCollection<NamedColorViewModel> NamedColors { get; } = [];

        /// <summary>
        /// Рекомендуемые ключи именованных цветов.
        /// </summary>
        public IReadOnlyList<string> NamedColorSuggestions { get; } =
        [
            KnownNamedColorRoles.Track,
            KnownNamedColorRoles.MaximumTrack,
            KnownNamedColorRoles.Thumb,
            KnownNamedColorRoles.Indicator,
            KnownNamedColorRoles.Ripple,
            KnownNamedColorRoles.Placeholder,
            KnownNamedColorRoles.Icon,
            KnownNamedColorRoles.Progress,
            KnownNamedColorRoles.Text,
            KnownNamedColorRoles.Background,
            KnownNamedColorRoles.Border
        ];

        /// <summary>
        /// Загружает редактор из темы.
        /// </summary>
        public void LoadFrom(ThemeDefinition theme, string controlTypeName, ComponentState componentState, ControlThemeDescriptor? descriptor = null)
        {
            ArgumentNullException.ThrowIfNull(theme);
            ArgumentException.ThrowIfNullOrWhiteSpace(controlTypeName);

            ControlType = controlTypeName;
            State = componentState;
            Title = $"{controlTypeName} · {componentState}";

            SupportsGradientBackground = descriptor?.SupportsGradientBackground ?? true;

            ComponentTheme component = theme.GetComponentTheme(controlTypeName, componentState);

            ForegroundHex = component.Foreground?.Hex ?? "#1F1F1F";
            BackgroundHex = component.Background?.Hex ?? "#FFFFFF";
            BorderHex = component.Border?.Hex ?? "#DADCE0";

            UseGradientBackground = component.BackgroundGradient is not null;
            GradientSettings.LoadFrom(component.BackgroundGradient);

            NamedColors.Clear();

            if (component.NamedColors is not null)
            {
                foreach (KeyValuePair<string,ColorToken> pair in component.NamedColors)
                {
                    NamedColors.Add(new NamedColorViewModel
                    {
                        Key = pair.Key,
                        Hex = pair.Value.Hex
                    });
                }
            }
        }

        /// <summary>
        /// Строит обновленный компонент, сохраняя typography/geometry/effects из существующего компонента.
        /// </summary>
        public ComponentTheme BuildComponentTheme(ThemeDefinition theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            string control = ControlType ?? KnownControlTypes.Button;
            ComponentTheme existing = theme.GetComponentTheme(control, State);
            Dictionary<string, ColorToken> namedColors = new (StringComparer.OrdinalIgnoreCase);

            foreach (NamedColorViewModel item in NamedColors)
            {
                if (string.IsNullOrWhiteSpace(item.Key)) continue;

                if (!ColorUtility.TryNormalizeHex(item.Hex, out string normalized)) continue;

                namedColors[item.Key] = new ColorToken
                {
                    Hex = normalized,
                    Source = ColorNameSource.User
                };
            }

            GradientTheme? gradient = UseGradientBackground && SupportsGradientBackground ? GradientSettings.BuildGradientTheme() : null;

            return existing with
            {
                ControlType = control,
                State = State,
                Foreground = CreateToken(ForegroundHex),
                Background = CreateToken(BackgroundHex),
                Border = CreateToken(BorderHex),
                BackgroundGradient = gradient,
                NamedColors = namedColors.Count == 0 ? null : namedColors
            };
        }

        /// <summary>
        /// Добавляет новую именованную цветовую роль.
        /// </summary>
        [RelayCommand]
        private void AddNamedColor()
        {
            string key = GetUniqueNamedColorKey("Color");

            NamedColors.Add(new NamedColorViewModel
            {
                Key = key,
                Hex = "#FFFFFF"
            });
        }

        /// <summary>
        /// Удаляет именованную цветовую роль.
        /// </summary>
        [RelayCommand]
        private void RemoveNamedColor(NamedColorViewModel? item)
        {
            if (item is null)
            {
                return;
            }

            NamedColors.Remove(item);
        }

        private string GetUniqueNamedColorKey(string baseKey)
        {
            string candidate = baseKey;
            int index = 1;

            while (NamedColors.Any(c => string.Equals(c.Key, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                candidate = $"{baseKey}{index++}";
            }

            return candidate;
        }

        private static ColorToken? CreateToken(string hex)
        {
            if (!ColorUtility.TryNormalizeHex(hex, out string normalized)) return null;

            return new ColorToken
            {
                Hex = normalized,
                Source = ColorNameSource.User
            };
        }
    }
}
