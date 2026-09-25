using ThemeForge.Maui.DI;

namespace MyMAUIUIThemeLibTester
{
    /// <summary>
    /// Приложение MAUI.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Создает приложение.
        /// </summary>
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        /// <inheritdoc />
        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState);

            window.Created += (_, _) =>
            {
                this.InitializeThemeForge();
            };

            return window;
        }
    }
}