using Gauniv.Client.Services;
using Gauniv.Client.ViewModel;
using Gauniv.Client.Data;

namespace Gauniv.Client.Pages
{
    public partial class Index : ContentPage
    {
        public IndexViewModel ViewModel { get; }

        public Index()
        {
            InitializeComponent();

            // Cr�ez des instances des services requis
            var gameService = new GameService();
            var userLibraryService = new UserLibraryService(); // Assurez-vous que ce service est disponible

            // Passez les deux services au constructeur d'IndexViewModel
            ViewModel = new IndexViewModel(gameService, userLibraryService);
            BindingContext = ViewModel;
        }

        // Le gestionnaire d'�v�nements pour le bouton de filtrage
        private async void OnFilterClicked(object sender, EventArgs e)
        {
            string searchTerm = searchTermEntry.Text;
            decimal? minPrice = string.IsNullOrWhiteSpace(minPriceEntry.Text) ? (decimal?)null : decimal.Parse(minPriceEntry.Text);
            decimal? maxPrice = string.IsNullOrWhiteSpace(maxPriceEntry.Text) ? (decimal?)null : decimal.Parse(maxPriceEntry.Text);

            // R�cup�rer la cat�gorie s�lectionn�e
            string selectedCategory = categoryPicker.SelectedItem?.ToString();

            // V�rifiez si "Toutes les cat�gories" est s�lectionn�
            if (selectedCategory == "Toutes les cat�gories")
            {
                selectedCategory = null; // Envoi null pour ne pas filtrer par cat�gorie
            }

            // Appeler la m�thode de filtrage avec les param�tres
            await ViewModel.LoadFilteredGames(searchTerm, minPrice, maxPrice, selectedCategory);
        }

        private async void OnResetClicked(object sender, EventArgs e)
        {
            // R�initialiser les champs de filtre
            searchTermEntry.Text = string.Empty;
            minPriceEntry.Text = string.Empty;
            maxPriceEntry.Text = string.Empty;
            categoryPicker.SelectedItem = null; // R�initialiser la s�lection du Picker

            string searchTerm = null;
            decimal? minPrice = null;
            decimal? maxPrice = null;
            string selectedCategory = null;

            // Charger tous les jeux sans param�tres
            await ViewModel.LoadFilteredGames(searchTerm, minPrice, maxPrice, selectedCategory);
        }

        private async void OnGameSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                // R�cup�rer le jeu s�lectionn�
                var selectedGame = e.CurrentSelection[0] as Game;

                // Afficher un pop-up avec la description du jeu
                if (selectedGame != null)
                {
                    await DisplayAlert(selectedGame.Title, selectedGame.Description, "OK");
                }
            }

            // D�s�lectionner l'�l�ment pour �viter que le pop-up ne se r�affiche lors de la navigation
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
