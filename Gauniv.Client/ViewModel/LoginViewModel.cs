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
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isBusy;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;

            // HttpClient setup
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };
        }

        [RelayCommand]
        async Task LoginAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                // Validation de l'email et du mot de passe
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter email and password", "OK");
                    return;
                }

                // Création des données de login
                var loginData = new { email = Email, password = Password };

                // Envoi de la requête de login
                var response = await _httpClient.PostAsJsonAsync("/Bearer/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    // Récupération du token (ou autre réponse)
                    var token = await response.Content.ReadAsStringAsync();

                    // Stockage du token via AuthService
                    _authService.SetToken(token);

                    // Navigation vers la page principale après succès
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
            // Open the registration URL in the default web browser
            await Browser.OpenAsync("https://localhost/Identity/Account/Register", BrowserLaunchMode.SystemPreferred);
        }
    }
}
