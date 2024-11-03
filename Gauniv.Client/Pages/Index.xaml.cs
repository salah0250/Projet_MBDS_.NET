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

            // Créez des instances des services requis
            var gameService = new GameService();
            var userLibraryService = new UserLibraryService(); // Assurez-vous que ce service est disponible

            // Passez les deux services au constructeur d'IndexViewModel
            ViewModel = new IndexViewModel(gameService, userLibraryService);
            BindingContext = ViewModel;
        }

        // Le gestionnaire d'événements pour le bouton de filtrage
        private async void OnFilterClicked(object sender, EventArgs e)
        {
            string searchTerm = searchTermEntry.Text;
            decimal? minPrice = string.IsNullOrWhiteSpace(minPriceEntry.Text) ? (decimal?)null : decimal.Parse(minPriceEntry.Text);
            decimal? maxPrice = string.IsNullOrWhiteSpace(maxPriceEntry.Text) ? (decimal?)null : decimal.Parse(maxPriceEntry.Text);

            // Récupérer la catégorie sélectionnée
            string selectedCategory = categoryPicker.SelectedItem?.ToString();

            // Vérifiez si "Toutes les catégories" est sélectionné
            if (selectedCategory == "Toutes les catégories")
            {
                selectedCategory = null; // Envoi null pour ne pas filtrer par catégorie
            }

            // Appeler la méthode de filtrage avec les paramètres
            await ViewModel.LoadFilteredGames(searchTerm, minPrice, maxPrice, selectedCategory);
        }

        private async void OnResetClicked(object sender, EventArgs e)
        {
            // Réinitialiser les champs de filtre
            searchTermEntry.Text = string.Empty;
            minPriceEntry.Text = string.Empty;
            maxPriceEntry.Text = string.Empty;
            categoryPicker.SelectedItem = null; // Réinitialiser la sélection du Picker

            string searchTerm = null;
            decimal? minPrice = null;
            decimal? maxPrice = null;
            string selectedCategory = null;

            // Charger tous les jeux sans paramètres
            await ViewModel.LoadFilteredGames(searchTerm, minPrice, maxPrice, selectedCategory);
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
}
