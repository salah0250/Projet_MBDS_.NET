using CommunityToolkit.Maui;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using Gauniv.Client.ViewModel;
using Microsoft.Extensions.Logging;
using Windows.Networking.NetworkOperators;

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
            // Register HttpClient as a singleton
            builder.Services.AddSingleton<HttpClient>();

            // Register GameService as a singleton
            builder.Services.AddSingleton<GameService>();

            // Register ViewModel
            builder.Services.AddTransient<IndexViewModel>();

            builder.Services.AddSingleton<IProfileService, ProfileService>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<Profile>();

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
