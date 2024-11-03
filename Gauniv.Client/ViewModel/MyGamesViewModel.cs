using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Storage;
using Gauniv.Client.Settings;
using System.Diagnostics;
using System.IO.Compression;
using Gauniv.Client.Data;

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel : ObservableObject
    {
        private readonly UserLibraryService _userLibraryService;
        private readonly AuthService _authService;
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private string defaultDownloadPath;

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public MyGamesViewModel()
        {
            _userLibraryService = new UserLibraryService();
            _authService = new AuthService();
            _settingsService = new SettingsService();
            DefaultDownloadPath = _settingsService.GetDefaultDownloadPath();
            LoadUserLibrary();
        }

        [RelayCommand]
        private async Task SetDefaultDownloadPath()
        {
            try
            {
                var folderResult = await FolderPicker.Default.PickAsync();
                if (folderResult?.Folder != null)
                {
                    DefaultDownloadPath = folderResult.Folder.Path;
                    _settingsService.SetDefaultDownloadPath(DefaultDownloadPath);
                    await Shell.Current.DisplayAlert("Succès", "Dossier de téléchargement mis à jour avec succès!", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"Impossible de définir le dossier de téléchargement: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task LaunchGame(Game game)
        {
            try
            {
                var gamePath = Path.Combine(DefaultDownloadPath, game.Title);
                if (!Directory.Exists(gamePath))
                {
                    await Shell.Current.DisplayAlert("Erreur", "Le jeu n'est pas installé.", "OK");
                    return;
                }

                // Assuming the executable is named the same as the game title
                var executablePath = Path.Combine(gamePath, $"{game.Title}.exe");
                if (!File.Exists(executablePath))
                {
                    await Shell.Current.DisplayAlert("Erreur", "Impossible de trouver l'exécutable du jeu.", "OK");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"Impossible de lancer le jeu: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteGame(Game game)
        {
            try
            {
                var result = await Shell.Current.DisplayAlert(
                    "Confirmation",
                    $"Êtes-vous sûr de vouloir supprimer {game.Title} ?",
                    "Oui",
                    "Non");

                if (!result) return;

                var gamePath = Path.Combine(DefaultDownloadPath, game.Title);
                if (Directory.Exists(gamePath))
                {
                    Directory.Delete(gamePath, true);
                    game.IsDownloaded = false;
                    // Trigger UI update
                    var index = Games.IndexOf(game);
                    Games[index] = game;
                    await Shell.Current.DisplayAlert("Succès", "Jeu supprimé avec succès!", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"Impossible de supprimer le jeu: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task DownloadGame(Game game)
        {
            try
            {
                var fileName = $"{game.Title}.zip";
                string selectedFolderPath;

                // Use default path if available, otherwise ask user
                if (!string.IsNullOrEmpty(DefaultDownloadPath) && Directory.Exists(DefaultDownloadPath))
                {
                    selectedFolderPath = DefaultDownloadPath;
                }
                else
                {
                    var folderResult = await FolderPicker.Default.PickAsync();
                    if (folderResult?.Folder == null)
                    {
                        return;
                    }
                    selectedFolderPath = folderResult.Folder.Path;
                }

                var filePath = Path.Combine(selectedFolderPath, fileName);
                var extractionPath = Path.Combine(selectedFolderPath, game.Title);

                var httpClient = _authService.GetHttpClient();
                var downloadUrl = $"https://localhost/api/1.0.0/Games/Download/{game.Id}";

                using var response = await httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, fileBytes);

                if (Directory.Exists(extractionPath))
                {
                    Directory.Delete(extractionPath, true);
                }

                ZipFile.ExtractToDirectory(filePath, extractionPath);
                File.Delete(filePath);

                game.IsDownloaded = true;
                // Trigger UI update
                var index = Games.IndexOf(game);
                Games[index] = game;

                await Shell.Current.DisplayAlert("Succès", "Jeu téléchargé et extrait avec succès!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur", $"Une erreur est survenue: {ex.Message}", "OK");
            }
        }

        public async Task LoadUserLibrary()
        {
            try
            {
                var games = await _userLibraryService.GetUserLibraryAsync();
                Games.Clear();
                foreach (var game in games)
                {
                    game.IsDownloaded = CheckIfGameIsDownloaded(game);
                    Games.Add(game);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading user library: {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger la bibliothèque", "OK");
            }
        }

        private bool CheckIfGameIsDownloaded(Game game)
        {
            if (string.IsNullOrEmpty(DefaultDownloadPath)) return false;
            var gamePath = Path.Combine(DefaultDownloadPath, game.Title);
            return Directory.Exists(gamePath);
        }
    }
}
