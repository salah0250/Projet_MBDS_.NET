using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using Index = Gauniv.Client.Pages.Index;

namespace Gauniv.Client
{
    public partial class AppShell : Shell
    {
        private readonly AuthService _authService;

        public AppShell(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            // Register routes
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("games", typeof(Index));
            Routing.RegisterRoute("profile", typeof(Profile));
            Routing.RegisterRoute("mygames", typeof(MyGames));

            // Subscribe to auth changes
            _authService.OnConnectionStatusChanged += AuthService_OnConnectionStatusChanged;

            // Initial navigation based on auth state
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await HandleAuthenticationState();
            });
        }

        private async void AuthService_OnConnectionStatusChanged(object sender, EventArgs e)
        {
            await HandleAuthenticationState();
        }

        private async Task HandleAuthenticationState()
        {
            if (_authService.IsConnected)
            {
                // Show main content
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//games");
                    Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                }
            }
            else
            {
                // Show login
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//login");
                    Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
                }
            }
        }

    }
}
