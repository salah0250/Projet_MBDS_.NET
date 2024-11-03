using Gauniv.Client.ViewModel;
using Gauniv.Client.Data;
namespace Gauniv.Client.Pages;
public partial class MyGames : ContentPage
{

    public MyGamesViewModel ViewModel { get; }
    public MyGames()
    {
        InitializeComponent();
        ViewModel = new MyGamesViewModel();
        BindingContext = ViewModel;
    }
    private async void OnGameSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var selectedGame = e.CurrentSelection[0] as Game;
            if (selectedGame != null)
            {
                await Navigation.PushModalAsync(new GameDetails(selectedGame));
            }
        }
        ((CollectionView)sender).SelectedItem = null;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadUserLibrary(); // Load the games each time the page appears
    }
}