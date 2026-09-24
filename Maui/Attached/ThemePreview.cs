using Abstractions.Enums;

namespace Maui.Attached
{
    /// <summary>
    /// Attached properties для управления режимом предпросмотра элементов.
    /// </summary>
    public static class ThemePreview
    {
        /// <summary>
        /// Свойство режима предпросмотра.
        /// </summary>
        public static readonly BindableProperty ModeProperty = BindableProperty.CreateAttached(
                "Mode",
                typeof(PreviewMode),
                typeof(ThemePreview),
                PreviewMode.Live,
                propertyChanged: OnModeChanged);

        private static readonly BindableProperty OriginalIsEnabledProperty = BindableProperty.CreateAttached(
                "OriginalIsEnabled",
                typeof(bool?),
                typeof(ThemePreview),
                null);

        /// <summary>
        /// Получает режим предпросмотра элемента.
        /// </summary>
        public static PreviewMode GetMode(BindableObject bindable) => (PreviewMode)bindable.GetValue(ModeProperty);

        /// <summary>
        /// Устанавливает режим предпросмотра элемента.
        /// </summary>
        public static void SetMode(BindableObject bindable, PreviewMode value) => bindable.SetValue(ModeProperty, value);

        /// <summary>
        /// Рекурсивно применяет режим предпросмотра ко всем потомкам элемента.
        /// </summary>
        /// <param name="root">Корневой визуальный элемент.</param>
        /// <param name="mode">Режим предпросмотра.</param>
        public static void ApplyToDescendants(VisualElement root, PreviewMode mode)
        {
            ArgumentNullException.ThrowIfNull(root);

            SetMode(root, mode);

            if (root is Layout layout)
            {
                foreach (IView child in layout.Children)
                {
                    if (child is VisualElement visualChild)
                    {
                        ApplyToDescendants(visualChild, mode);
                    }
                }
            }
        }

        private static void OnModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not VisualElement element) return;

            PreviewMode mode = (PreviewMode)newValue;

            if (mode == PreviewMode.Live)
            {
                bool? original = (bool?)element.GetValue(OriginalIsEnabledProperty);

                if (original.HasValue)
                {
                    element.IsEnabled = original.Value;
                    element.SetValue(OriginalIsEnabledProperty, null);
                }

                return;
            }

            bool? storedOriginal = (bool?)element.GetValue(OriginalIsEnabledProperty);

            if (!storedOriginal.HasValue)
            {
                element.SetValue(OriginalIsEnabledProperty, element.IsEnabled);
            }

            element.IsEnabled = mode == PreviewMode.Active;
        }
    }
}
