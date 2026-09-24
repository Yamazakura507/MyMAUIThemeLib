using Maui.DI;

namespace MyMAUIUIThemeLibTester
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell());

            window.Created += (_, _) =>
            {
                this.InitializeThemeForge();
            };

            return window;
        }
    }
}