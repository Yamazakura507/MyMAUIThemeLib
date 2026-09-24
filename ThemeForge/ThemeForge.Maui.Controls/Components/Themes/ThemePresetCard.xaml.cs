using System.Windows.Input;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfColors;
using ThemeForge.Abstractions.Records.UseOfColors.Gradients;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Maui.Helpers;
using ThemeForge.Maui.Services;
using GradientStop = ThemeForge.Abstractions.Records.UseOfColors.Gradients.GradientStop;

namespace ThemeForge.Maui.Controls.Components.Themes;

/// <summary>
/// Карточка готовой темы с локальным стилированием и overlay-действиями.
/// </summary>
public partial class ThemePresetCard : ContentView
{
    /// <summary>
    /// Bindable-свойство темы.
    /// </summary>
    public static readonly BindableProperty ThemeProperty = BindableProperty.Create(
                                                                            nameof(Theme),
                                                                            typeof(ThemeDefinition),
                                                                            typeof(ThemePresetCard),
                                                                            null,
                                                                            propertyChanged: OnThemeChanged);

    /// <summary>
    /// Bindable-свойство режима отображения карточки.
    /// </summary>
    public static readonly BindableProperty PresentationModeProperty = BindableProperty.Create(
                                                                            nameof(PresentationMode),
                                                                            typeof(ThemePresentationMode),
                                                                            typeof(ThemePresetCard),
                                                                            ThemePresentationMode.Automatic,
                                                                            propertyChanged: OnPresentationModeChanged);

    /// <summary>
    /// Bindable-свойство команды применения.
    /// </summary>
    public static readonly BindableProperty ApplyCommandProperty = BindableProperty.Create(
                                                                            nameof(ApplyCommand),
                                                                            typeof(ICommand),
                                                                            typeof(ThemePresetCard));

    /// <summary>
    /// Bindable-свойство команды добавления в пресеты.
    /// </summary>
    public static readonly BindableProperty FavoriteCommandProperty = BindableProperty.Create(
                                                                            nameof(FavoriteCommand),
                                                                            typeof(ICommand),
                                                                            typeof(ThemePresetCard));

    /// <summary>
    /// Bindable-свойство команды удаления.
    /// </summary>
    public static readonly BindableProperty DeleteCommandProperty = BindableProperty.Create(
                                                                            nameof(DeleteCommand),
                                                                            typeof(ICommand),
                                                                            typeof(ThemePresetCard));

    private bool overlayVisible;

    /// <summary>
    /// Тема, отображаемая карточкой.
    /// </summary>
    public ThemeDefinition? Theme
    {
        get => (ThemeDefinition?)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    /// <summary>
    /// Режим отображения карточки.
    /// </summary>
    public ThemePresentationMode PresentationMode
    {
        get => (ThemePresentationMode)GetValue(PresentationModeProperty);
        set => SetValue(PresentationModeProperty, value);
    }

    /// <summary>
    /// Команда применения темы.
    /// </summary>
    public ICommand? ApplyCommand
    {
        get => (ICommand?)GetValue(ApplyCommandProperty);
        set => SetValue(ApplyCommandProperty, value);
    }

    /// <summary>
    /// Команда добавления темы в пресеты.
    /// </summary>
    public ICommand? FavoriteCommand
    {
        get => (ICommand?)GetValue(FavoriteCommandProperty);
        set => SetValue(FavoriteCommandProperty, value);
    }

    /// <summary>
    /// Команда удаления темы.
    /// </summary>
    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    /// <summary>
    /// Доступные режимы отображения.
    /// </summary>
    public IReadOnlyList<ThemePresentationMode> Modes { get; } =
    [
        ThemePresentationMode.Automatic,
        ThemePresentationMode.Solid,
        ThemePresentationMode.LinearGradient,
        ThemePresentationMode.RadialGradient,
        ThemePresentationMode.Animated
    ];

    /// <summary>
    /// Команда показа overlay.
    /// </summary>
    public ICommand ShowOverlayCommand { get; }

    /// <summary>
    /// Команда скрытия overlay.
    /// </summary>
    public ICommand HideOverlayCommand { get; }

    /// <summary>
    /// Создает карточку темы.
    /// </summary>
    public ThemePresetCard()
    {
        InitializeComponent();

        BindingContext = this;

        ShowOverlayCommand = new Command(async () => await SetOverlayAsync(true));
        HideOverlayCommand = new Command(async () => await SetOverlayAsync(false));
    }

    /// <inheritdoc />
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        UpdateThemeResources();
        UpdateDisplayBrush();
        UpdateTitleAndDetail();
    }

    private static void OnThemeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemePresetCard card)
        {
            card.UpdateThemeResources();
            card.UpdateDisplayBrush();
            card.UpdateTitleAndDetail();
        }
    }

    private static void OnPresentationModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemePresetCard card)
        {
            card.UpdateDisplayBrush();
        }
    }

    private void UpdateThemeResources()
    {
        Resources.MergedDictionaries.Clear();

        if (Theme is null) return;

        ThemeResourceBuilder? builder = GetService<ThemeResourceBuilder>();

        if (builder is null) return;

        Resources.MergedDictionaries.Add(builder.Build(Theme));
    }

    private void UpdateDisplayBrush()
    {
        if (Theme is null)
        {
            Resources["Theme.Card.DisplayBrush"] = new SolidColorBrush(Colors.Gray);

            return;
        }

        Brush brush = CreateDisplayBrush(Theme, PresentationMode);
        Resources["Theme.Card.DisplayBrush"] = brush;
    }

    private void UpdateTitleAndDetail()
    {
        if (Theme is null)
        {
            TitleLabel.Text = string.Empty;
            DetailLabel.Text = string.Empty;
            return;
        }

        IThemeNamingService? naming = GetService<IThemeNamingService>();

        TitleLabel.Text = naming?.BuildPresetTitle(Theme) ?? Theme.Name;
        DetailLabel.Text = naming?.BuildPresetDetail(Theme) ?? Theme.Kind.ToString();
    }

    private Brush CreateDisplayBrush(ThemeDefinition theme, ThemePresentationMode mode) => mode switch
    {
        ThemePresentationMode.Solid => ColorConversion.ToSolidBrush(GetPrimaryHex(theme)),
        ThemePresentationMode.Automatic or ThemePresentationMode.Animated when theme.Gradient is not null => theme.Gradient.ToBrush(),
        ThemePresentationMode.LinearGradient => CreateSyntheticGradient(theme, GradientType.Linear).ToBrush(),
        ThemePresentationMode.RadialGradient => CreateSyntheticGradient(theme, GradientType.Radial).ToBrush(),
        _ when theme.Gradient is not null => theme.Gradient.ToBrush(),
        _ => ColorConversion.ToSolidBrush(GetPrimaryHex(theme))
    };

    private static GradientTheme CreateSyntheticGradient(ThemeDefinition theme, GradientType type)
    {
        IReadOnlyList<string> colors = GetDisplayColors(theme);
        List<GradientStop> stops = new (colors.Count);

        for (int i = 0; i < colors.Count; i++)
        {
            double offset = colors.Count == 1 ? 0 : (double)i / (colors.Count - 1);

            stops.Add(new GradientStop(ColorToken.FromHex(colors[i]), offset));
        }

        GradientGeometry geometry = type switch
        {
            GradientType.Linear => new GradientGeometry
            {
                AngleDegrees = 90,
                StartPoint = new NormalizedPoint(0, 0.5),
                EndPoint = new NormalizedPoint(1, 0.5)
            },
            GradientType.Radial => new GradientGeometry
            {
                CenterPoint = new NormalizedPoint(0.5, 0.5),
                Radius = 0.75
            },
            _ => new GradientGeometry()
        };

        return new GradientTheme(type, stops) { Geometry = geometry };
    }

    private static IReadOnlyList<string> GetDisplayColors(ThemeDefinition theme)
    {
        if (theme.Gradient is not null && theme.Gradient.Stops.Count > 0)
        {
            return theme.Gradient.Stops.Select(s => s.Color.Hex).ToList();
        }

        if (theme.Solid is not null)
        {
            if (theme.Solid.Palette.Count > 0)
            {
                return theme.Solid.Palette.Select(p => p.Hex).ToList();
            }

            string baseHex = theme.Solid.BaseColor.Hex;

            return
            [
                baseHex,
                ColorConversion.Lighten(baseHex, 0.12),
                ColorConversion.Darken(baseHex, 0.12)
            ];
        }

        return ["#1976D2"];
    }

    private static string GetPrimaryHex(ThemeDefinition theme)
    {
        if (theme.Solid is not null)
        {
            return theme.Solid.BaseColor.Hex;
        }

        if (theme.Gradient is not null && theme.Gradient.Stops.Count > 0)
        {
            return theme.Gradient.Stops[0].Color.Hex;
        }

        return "#1976D2";
    }

    private async Task SetOverlayAsync(bool show)
    {
        if (overlayVisible == show) return;

        overlayVisible = show;

        if (show)
        {
            Backdrop.IsVisible = true;
            OverlayGrid.IsVisible = true;

            await OverlayGrid.RotateYToAsync(0, 220, Easing.CubicOut);
            await OverlayGrid.FadeToAsync(1, 180);
            await Backdrop.FadeToAsync(1, 180);
        }
        else
        {
            await OverlayGrid.RotateYToAsync(-90, 180, Easing.CubicIn);
            await OverlayGrid.FadeToAsync(0, 150);
            await Backdrop.FadeToAsync(0, 150);

            OverlayGrid.IsVisible = false;
            Backdrop.IsVisible = false;
        }
    }

    private async void OnPointerEntered(object? sender, PointerEventArgs e) => await SetOverlayAsync(true);

    private async void OnPointerExited(object? sender, PointerEventArgs e) => await SetOverlayAsync(false);

    private async void OnApplyClicked(object? sender, EventArgs e)
    {
        if (Theme is null) return;

        if (ApplyCommand is not null && ApplyCommand.CanExecute(Theme))
        {
            ApplyCommand.Execute(Theme);
        }
        else
        {
            IThemeService? themeService = GetService<IThemeService>();

            if (themeService is not null)
            {
                await themeService.UpdateDraftAsync(Theme);
                await themeService.ApplyAsync();
            }
        }

        await SetOverlayAsync(false);
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        if (Theme is null) return;

        if (FavoriteCommand is not null && FavoriteCommand.CanExecute(Theme))
        {
            FavoriteCommand.Execute(Theme);
        }

        await SetOverlayAsync(false);
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (Theme is null) return;

        if (DeleteCommand is not null && DeleteCommand.CanExecute(Theme))
        {
            DeleteCommand.Execute(Theme);
        }

        await SetOverlayAsync(false);
    }

    private T? GetService<T>() where T : class => this.Handler?.MauiContext?.Services.GetService<T>() ?? Application.Current?.Handler?.MauiContext?.Services.GetService<T>();
}