using Gauniv.Client.Services;

namespace Gauniv.Client
{
    public partial class App : Application
    {
        private readonly AuthService _authService;

        public App(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Passer l'instance d'AuthService lors de la création d'AppShell
            return new Window(new AppShell(_authService));
        }
    }
}
