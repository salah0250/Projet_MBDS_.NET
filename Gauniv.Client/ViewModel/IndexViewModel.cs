using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Data;
using Gauniv.Client.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class IndexViewModel : ObservableObject
    {
        private readonly GameService _gameService;
        private readonly UserLibraryService _userLibraryService;

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public IndexViewModel(GameService gameService, UserLibraryService userLibraryService)
        {
            _gameService = gameService;
            _userLibraryService = userLibraryService;
            LoadGames();
        }

        [RelayCommand]
        public async Task PurchaseGameAsync(Game game)
        {
            if (game == null) return;

            try
            {
                var success = await _gameService.PurchaseGameAsync(game.Id);
                if (success)
                {
                    // Refresh games list after purchase
                    await LoadFilteredGames(null, null, null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error purchasing game: {ex.Message}");
            }
        }

        private async void LoadGames()
        {
            try
            {
                var games = await _gameService.GetAllGamesAsync();
                var userLibrary = await _userLibraryService.GetUserLibraryAsync();

                Games.Clear();
                foreach (var game in games)
                {
                    // Check if the game is already in the user's library
                    game.IsNotInLibrary = !userLibrary.Any(ug => ug.Id == game.Id);
                    Games.Add(game);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading games: {ex.Message}");
            }
        }

        public async Task LoadFilteredGames(string searchTerm, decimal? minPrice, decimal? maxPrice, string category)
        {
            try
            {
                var filteredGames = await _gameService.GetFilteredGamesAsync(searchTerm, minPrice, maxPrice, category);
                var userLibrary = await _userLibraryService.GetUserLibraryAsync();

                Games.Clear();
                foreach (var game in filteredGames)
                {
                    // Check if the game is already in the user's library
                    game.IsNotInLibrary = !userLibrary.Any(ug => ug.Id == game.Id);
                    Games.Add(game);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading filtered games: {ex.Message}");
            }
        }
    }
}
