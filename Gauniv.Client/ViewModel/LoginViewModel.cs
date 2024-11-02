using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using System.Text.Json;
using System.Net.Http.Headers;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;
using Gauniv.Client.Services;

namespace Gauniv.Client.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly NetworkService _networkService;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            _networkService = NetworkService.Instance;
        }

        [RelayCommand]
        async Task LoginAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter email and password", "OK");
                    return;
                }

                var loginData = new { email = Email, password = Password };
                var response = await _networkService.httpClient.PostAsJsonAsync("/Bearer/login?useCookies=true&useSessionCookies=true", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    _authService.SetToken(token);
                    await Shell.Current.GoToAsync("//games");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlert("Error", $"Login failed: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task CreateAccountAsync()
        {
            await Browser.OpenAsync("https://localhost/Identity/Account/Register", BrowserLaunchMode.SystemPreferred);
        }
    }
}
