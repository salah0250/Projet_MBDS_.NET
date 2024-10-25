using CommunityToolkit.Maui;
using Gauniv.Client.Services;
using Microsoft.Extensions.Logging;

namespace Gauniv.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>().UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            Task.Run(() =>
            {
                // Vous pouvez initialiser la connection au serveur a partir d'ici
            });
            return app;
        }
    }
}
