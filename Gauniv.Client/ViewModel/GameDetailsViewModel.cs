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
        private async Task Close()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
