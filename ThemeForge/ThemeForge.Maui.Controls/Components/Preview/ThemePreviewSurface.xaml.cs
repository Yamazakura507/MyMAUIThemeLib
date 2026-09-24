using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Maui.Attached;
using ThemeForge.Maui.Services;

namespace ThemeForge.Maui.Controls.Components.Preview;

/// <summary>
/// Универсальная поверхность предпросмотра интерфейса.
/// </summary>
public partial class ThemePreviewSurface : ContentView
{
    /// <summary>
    /// Bindable-свойство темы предпросмотра.
    /// </summary>
    public static readonly BindableProperty ThemeProperty = BindableProperty.Create(
                                                                    nameof(Theme),
                                                                    typeof(ThemeDefinition),
                                                                    typeof(ThemePreviewSurface),
                                                                    null,
                                                                    propertyChanged: OnThemeChanged);

    /// <summary>
    /// Bindable-свойство режима предпросмотра.
    /// </summary>
    public static readonly BindableProperty ModeProperty = BindableProperty.Create(
                                                                    nameof(Mode),
                                                                    typeof(PreviewMode),
                                                                    typeof(ThemePreviewSurface),
                                                                    PreviewMode.Active,
                                                                    propertyChanged: OnModeChanged);

    /// <summary>
    /// Тема, отображаемая в предпросмотре.
    /// </summary>
    public ThemeDefinition? Theme
    {
        get => (ThemeDefinition?)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    /// <summary>
    /// Режим предпросмотра.
    /// </summary>
    public PreviewMode Mode
    {
        get => (PreviewMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>
    /// Создает поверхность предпросмотра.
    /// </summary>
    public ThemePreviewSurface()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private static void OnThemeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemePreviewSurface surface)
        {
            surface.UpdateThemeResources();
        }
    }

    private static void OnModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemePreviewSurface surface)
        {
            surface.ApplyPreviewMode();
        }
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        UpdateThemeResources();
        ApplyPreviewMode();
    }

    private void UpdateThemeResources()
    {
        Resources.MergedDictionaries.Clear();

        if (Theme is null) return;

        ThemeResourceBuilder? builder = GetService<ThemeResourceBuilder>();

        if (builder is not null)
        {
            Resources.MergedDictionaries.Add(builder.Build(Theme));
        }
    }

    private void ApplyPreviewMode() => ThemePreview.ApplyToDescendants(PreviewRoot, Mode);

    private T? GetService<T>() where T : class => this.Handler?.MauiContext?.Services.GetService<T>() ?? Application.Current?.Handler?.MauiContext?.Services.GetService<T>();
}