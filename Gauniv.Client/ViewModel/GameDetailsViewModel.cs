using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Data;
using Gauniv.Client.Services;
using Microsoft.Maui.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class GameDetailsViewModel : ObservableObject
    {
        [ObservableProperty]
        private Game game;

        private readonly AuthService _authService;

        public GameDetailsViewModel()
        {
            _authService = new AuthService();
        }

        [RelayCommand]
        private async Task Download()
        {
            if (game != null)
            {
                var fileName = $"{game.Title}.zip";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                try
                {
                    var httpClient = _authService.GetHttpClient();
                    var downloadUrl = $"https://localhost/api/1.0.0/Games/Download/{game.Id}";

                    var response = await httpClient.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    using (var stream = new MemoryStream(fileBytes))
                    {
                        await FileSaver.Default.SaveAsync(filePath, stream);
                    }

                    await Shell.Current.DisplayAlert("Succès", "Téléchargement terminé", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Erreur", $"Échec du téléchargement : {ex.Message}", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task Close()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
