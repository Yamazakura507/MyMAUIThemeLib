using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Records.UseOfEffects;

namespace ThemeForge.Maui.Controls.Components.Lottie;

/// <summary>
/// Хост фона темы: Skia-эффекты + Lottie + fallback.
/// </summary>
public partial class ThemeBackgroundHost : ContentView
{
    /// <summary>
    /// Bindable-свойство отображаемого brush.
    /// </summary>
    public static readonly BindableProperty DisplayBrushProperty = BindableProperty.Create(
                                                                        nameof(DisplayBrush),
                                                                        typeof(Brush),
                                                                        typeof(ThemeBackgroundHost),
                                                                        null,
                                                                        propertyChanged: OnDisplayBrushChanged);

    /// <summary>
    /// Bindable-свойство эффекта.
    /// </summary>
    public static readonly BindableProperty EffectProperty = BindableProperty.Create(
                                                                    nameof(Effect),
                                                                    typeof(EffectSettings),
                                                                    typeof(ThemeBackgroundHost),
                                                                    null,
                                                                    propertyChanged: OnEffectChanged);

    /// <summary>
    /// Bindable-свойство запуска анимации.
    /// </summary>
    public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(
                                                                        nameof(IsRunning),
                                                                        typeof(bool),
                                                                        typeof(ThemeBackgroundHost),
                                                                        true,
                                                                        propertyChanged: OnIsRunningChanged);

    /// <summary>
    /// Отображаемый фон.
    /// </summary>
    public Brush? DisplayBrush
    {
        get => (Brush?)GetValue(DisplayBrushProperty);
        set => SetValue(DisplayBrushProperty, value);
    }

    /// <summary>
    /// Эффект фона.
    /// </summary>
    public EffectSettings? Effect
    {
        get => (EffectSettings?)GetValue(EffectProperty);
        set => SetValue(EffectProperty, value);
    }

    /// <summary>
    /// Включена ли анимация.
    /// </summary>
    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    /// <summary>
    /// Создает хост фона.
    /// </summary>
    public ThemeBackgroundHost()
    {
        InitializeComponent();

        LottieHost.SourceAvailabilityChanged += OnLottieSourceAvailabilityChanged;
    }

    /// <inheritdoc />
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        UpdateRouting();
    }

    private static void OnDisplayBrushChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemeBackgroundHost host)
        {
            host.SkiaCanvas.DisplayBrush = host.DisplayBrush;
        }
    }

    private static void OnEffectChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemeBackgroundHost host)
        {
            host.UpdateRouting();
        }
    }

    private static void OnIsRunningChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThemeBackgroundHost host)
        {
            host.UpdateRouting();
        }
    }

    private void OnLottieSourceAvailabilityChanged(object? sender, EventArgs e) => UpdateRouting();

    private void UpdateRouting()
    {
        SkiaCanvas.DisplayBrush = DisplayBrush;
        SkiaCanvas.IsRunning = IsRunning;
        LottieHost.IsRunning = IsRunning;

        if (Effect is { IsEnabled: true, Kind: EffectKind.Lottie } lottieEffect)
        {
            LottieHost.Effect = lottieEffect;

            if (LottieHost.HasSource)
            {
                LottieHost.IsVisible = true;
                SkiaCanvas.Effect = null;
            }
            else
            {
                LottieHost.IsVisible = false;
                SkiaCanvas.Effect = lottieEffect with
                {
                    Kind = EffectKind.MovingColors
                };
            }

            return;
        }

        LottieHost.Effect = null;
        LottieHost.IsVisible = false;
        SkiaCanvas.Effect = Effect;
    }
}