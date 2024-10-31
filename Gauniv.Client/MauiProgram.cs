using CommunityToolkit.Maui;
using Gauniv.Client.Services;
using Gauniv.Client.ViewModel;
using Microsoft.Extensions.Logging;

namespace Gauniv.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddSingleton<AppShell>();
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
