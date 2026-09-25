using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ThemeForge.Maui.DI;
using ThemeForge.Maui.Studio.DI;
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
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .UseThemeForge(options =>
                {
                    options.StorageDirectory = FileSystem.AppDataDirectory;
                    options.StorageFileName = "theme-forge-themes.json";
                    options.SeedBuiltInThemes = true;
                })
                .UseThemeForgeTheColorApi(builder.Configuration.GetSection("ThemeForge:TheColor"))
                .UseThemeForgeStudio();

            #if DEBUG
                builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
