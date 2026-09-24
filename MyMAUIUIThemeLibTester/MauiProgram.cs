using Microsoft.Extensions.Logging;
using Maui.DI;

namespace MyMAUIUIThemeLibTester
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseThemeForge(options =>
                {
                    // Локальное хранилище тем.
                    // Если не задать, будет использоваться in-memory хранилище.
                    options.StorageDirectory = FileSystem.AppDataDirectory;
                    options.SeedBuiltInThemes = true;
                });

            #if DEBUG
                builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
