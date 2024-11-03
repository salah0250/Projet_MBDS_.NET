namespace Gauniv.Client.Pages;
using Gauniv.Client.Data;
using Gauniv.Client.ViewModel;

public partial class GameDetails : ContentPage
{
    private Game game;

    public GameDetails(Game game)
    {
        InitializeComponent();
        this.game = game;
        var viewModel = new GameDetailsViewModel
        {
            Game = game
        };
        BindingContext = viewModel;
    }
}
