using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Data;
using Gauniv.Client.Services;
using Microsoft.Maui.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
                try
                {
                    // Let the user choose the folder for download
                    var folderResult = await FolderPicker.Default.PickAsync();
                    if (folderResult == null)
                    {
                        // User canceled folder selection
                        return;
                    }

                    var selectedFolderPath = folderResult.Folder.Path;
                    var fileName = $"{game.Title}.zip";
                    var filePath = Path.Combine(selectedFolderPath, fileName);
                    var extractionPath = Path.Combine(selectedFolderPath, game.Title);

                    var httpClient = _authService.GetHttpClient();
                    var downloadUrl = $"https://localhost/api/1.0.0/Games/Download/{game.Id}";

                    // Download the zip file
                    var response = await httpClient.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, fileBytes);

                    // Extract the downloaded zip file to the selected folder
                    if (Directory.Exists(extractionPath))
                    {
                        Directory.Delete(extractionPath, true); // Clear any existing directory with the same name
                    }

                    ZipFile.ExtractToDirectory(filePath, extractionPath);

                    // Delete the zip file after extraction
                    File.Delete(filePath);

                    // Notify the user about the extraction
                    await Shell.Current.DisplayAlert("Success", "Download and extraction completed successfully!", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"Download failed: {ex.Message}", "OK");
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
