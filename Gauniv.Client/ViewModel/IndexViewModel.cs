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

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public IndexViewModel(GameService gameService)
        {
            _gameService = gameService;
            LoadGames();
        }

        private async void LoadGames()
        {
            try
            {
                var games = await _gameService.GetAllGamesAsync();
                Games.Clear();
                foreach (var game in games)
                {
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
                Games.Clear();
                foreach (var game in filteredGames)
                {
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
