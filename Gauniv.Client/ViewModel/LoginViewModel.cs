using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using System.Text.Json;
using System.Net.Http.Headers;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;
namespace Gauniv.Client.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        [ObservableProperty]
        private string email;
        [ObservableProperty]
        private string password;
        [ObservableProperty]
        private bool isBusy;
        public LoginViewModel()
        {
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
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    await Shell.Current.DisplayAlert("Error", "Please enter email and password", "OK");
                    return;
                }
                var loginData = new
                {
                    email = Email,
                    password = Password
                };
                var response = await _httpClient.PostAsJsonAsync("/Bearer/login", loginData);
                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    // Store token securely here if needed
                    // await SecureStorage.SetAsync("auth_token", token);
                    // Navigate to main page after successful login
                    await Shell.Current.GoToAsync("//games");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlert("Error", "Login failed. Please try again.", "OK");
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
    }
}