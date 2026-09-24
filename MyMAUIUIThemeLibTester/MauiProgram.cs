using Microsoft.Extensions.Logging;
using ThemeForge.Maui.DI;
using ThemeForge.Services.Maui.TheColor.DI;

namespace MyMAUIUIThemeLibTester
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseThemeForge(options =>
                {
                    options.StorageDirectory = FileSystem.AppDataDirectory;
                    options.SeedBuiltInThemes = true;
                })
                .UseThemeForgeTheColorApi(builder.Configuration.GetSection("ThemeForge:TheColor"));

            #if DEBUG
                builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
