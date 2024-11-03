using Gauniv.Client.ViewModel;
using Gauniv.Client.Data;
namespace Gauniv.Client.Pages;

public partial class MyGames : ContentPage
{
	public MyGames()
	{
		InitializeComponent();
        BindingContext = new MyGamesViewModel(); // Assurez-vous que cela soit bien défini.

    }

    private async void OnGameSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            // Récupérer le jeu sélectionné
            var selectedGame = e.CurrentSelection[0] as Game;

            // Afficher un pop-up avec la description du jeu
            if (selectedGame != null)
            {
                await DisplayAlert(selectedGame.Title, selectedGame.Description, "OK");
            }
        }

    // Désélectionner l'élément pour éviter que le pop-up ne se réaffiche lors de la navigation
    ((CollectionView)sender).SelectedItem = null;
    }

}