using ThemeForge.Maui.Studio.ComponentModels;

namespace ThemeForge.Maui.Studio.Pages;

/// <summary>
/// Центральная страница настройки тем.
/// </summary>
public partial class ThemeStudioPage : ContentPage
{
    /// <summary>
    /// Создает страницу студии.
    /// </summary>
    public ThemeStudioPage()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ThemeStudioViewModel viewModel)
        {
            _ = viewModel.InitializeCommand.ExecuteAsync(null);
            return;
        }

        IServiceProvider? services = Application.Current?.Handler?.MauiContext?.Services;

        if (services is null) return;

        viewModel = services.GetService<ThemeStudioViewModel>();

        if (viewModel is null) return;

        BindingContext = viewModel;

        _ = viewModel.InitializeCommand.ExecuteAsync(null);
    }
}