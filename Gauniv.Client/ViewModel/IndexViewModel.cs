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
using Gauniv.Client.Data;   

namespace Gauniv.Client.ViewModel
{
    public partial class IndexViewModel : ObservableObject
    {
        private readonly GameService _gameService;

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public IndexViewModel() { } // Parameterless constructor

        public IndexViewModel(GameService gameService) // Constructor with dependency injection
        {
            _gameService = gameService;
            LoadGames();
        }

        private async void LoadGames()
        {
            try
            {
                if (_gameService != null)
                {
                    var games = await _gameService.GetAllGamesAsync();
                    Games.Clear();
                    foreach (var game in games)
                    {
                        Games.Add(game);
                        Debug.WriteLine($"Loaded game: {game.Title}");
                    }
            }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading games: {ex.Message}");
            }
        }

    }
}
