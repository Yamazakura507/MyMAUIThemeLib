using System.Windows.Input;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Maui.Controls.Helpers;
using ThemeForge.Maui.Services;

namespace ThemeForge.Maui.Controls.Components.Themes;

/// <summary>
/// Карточка готовой темы с локальным стилированием, blur-overlay и эффектами.
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
        UpdateDisplay();
        UpdateTitleAndDetail();
    }

    private static void OnThemeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemePresetCard card)
        {
            card.UpdateThemeResources();
            card.UpdateDisplay();
            card.UpdateTitleAndDetail();
        }
    }

    private static void OnPresentationModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemePresetCard card)
        {
            card.UpdateDisplay();
        }
    }

    private void UpdateThemeResources()
    {
        Resources.MergedDictionaries.Clear();

        if (Theme is null) return;

        ThemeResourceBuilder? builder = this.GetService<ThemeResourceBuilder>();

        if (builder is null) return;

        Resources.MergedDictionaries.Add(builder.Build(Theme));
    }

    private void UpdateDisplay()
    {
        if (Theme is null)
        {
            BackgroundHost.DisplayBrush = null;
            BackgroundHost.Effect = null;
            return;
        }

        Brush brush = ThemeDisplayBrushFactory.CreateDisplayBrush(Theme, PresentationMode);
        EffectSettings? effect = ThemeDisplayBrushFactory.ResolveEffect(Theme);

        BackgroundHost.DisplayBrush = brush;
        BackgroundHost.Effect = effect;
    }

    private void UpdateTitleAndDetail()
    {
        if (Theme is null)
        {
            TitleLabel.Text = string.Empty;
            DetailLabel.Text = string.Empty;
            return;
        }

        IThemeNamingService? naming = this.GetService<IThemeNamingService>();

        TitleLabel.Text = naming?.BuildPresetTitle(Theme) ?? Theme.Name;
        DetailLabel.Text = naming?.BuildPresetDetail(Theme) ?? Theme.Kind.ToString();
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
            IThemeService? themeService = this.GetService<IThemeService>();

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
}