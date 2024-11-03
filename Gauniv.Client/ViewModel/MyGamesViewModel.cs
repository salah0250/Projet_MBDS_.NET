using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;
using Gauniv.Client.Data;
using System.IO.Compression;

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel : ObservableObject
    {
        private readonly UserLibraryService _userLibraryService;
        private readonly AuthService _authService;
        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public MyGamesViewModel()
        {
            _userLibraryService = new UserLibraryService();
            _authService = new AuthService();
            LoadUserLibrary();
        }

        // Créer une commande RelayCommand pour le téléchargement
        public IRelayCommand<Game> DownloadGameCommand => new RelayCommand<Game>(async (game) => await DownloadGameAsync(game));

        private async Task DownloadGameAsync(Game game)
        {
            try
            {
                var fileName = $"{game.Title}.zip";

                // Let the user choose the folder for download
                var folderResult = await FolderPicker.Default.PickAsync();
                if (folderResult == null)
                {
                    // User canceled folder selection
                    return;
                }

                var selectedFolderPath = folderResult.Folder.Path;
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
                await Shell.Current.DisplayAlert("Success", "Game downloaded and extracted successfully!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }


        private async void LoadUserLibrary()
        {
            try
            {
                var games = await _userLibraryService.GetUserLibraryAsync();
                Debug.WriteLine($"Games count: {games.Count()}"); // Affiche le nombre de jeux récupérés.
                Games.Clear();
                if (games.Any())
                {
                    foreach (var game in games)
                    {
                        Games.Add(game);
                        Debug.WriteLine($"Loaded game: {game.Title}");
                    }
                }
                else
                {
                    Debug.WriteLine("No games found in the user's library.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading user library: {ex.Message}");
            }
        }
    }
}
