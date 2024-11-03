using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gauniv.Client.Data;
using System.IO; // Pour l'accès aux fichiers
using System.Threading.Tasks; // Pour les méthodes async

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel : ObservableObject
    {
        private readonly UserLibraryService _userLibraryService;

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public MyGamesViewModel()
        {
            _userLibraryService = new UserLibraryService();
            LoadUserLibrary();
        }


        // Créer une commande RelayCommand pour le téléchargement
        public IRelayCommand<Game> DownloadGameCommand => new RelayCommand<Game>(async (game) => await DownloadGameAsync(game));

        private async Task DownloadGameAsync(Game game)
        {
            if (game.Payload != null)
            {
                var fileName = $"{game.Title}.game"; // Changez l'extension si nécessaire
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                try
                {
                    await File.WriteAllBytesAsync(filePath, game.Payload);
                    Debug.WriteLine($"Game downloaded to: {filePath}");
                    // Afficher un message à l'utilisateur
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error downloading game: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("No payload available for download.");
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
