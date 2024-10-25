using Gauniv.Client.Services;
using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _authService;

        public LoginPage(AuthService authService)
        {
            InitializeComponent();

            _authService = authService;

            BindingContext = new LoginViewModel(_authService);
        }
    }
}
