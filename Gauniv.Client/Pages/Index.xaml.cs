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

            // Create instances of required services
            var gameService = new GameService();
            var userLibraryService = new UserLibraryService();

            // Pass the services to the constructor of IndexViewModel
            ViewModel = new IndexViewModel(gameService, userLibraryService);
            BindingContext = ViewModel;
        }

        // Called when the page appears
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.LoadGames(); // Load the games each time the page appears
        }

        // The event handler for the filter button
        private async void OnFilterClicked(object sender, EventArgs e)
        {
            string searchTerm = searchTermEntry.Text;
            decimal? minPrice = string.IsNullOrWhiteSpace(minPriceEntry.Text) ? (decimal?)null : decimal.Parse(minPriceEntry.Text);
            decimal? maxPrice = string.IsNullOrWhiteSpace(maxPriceEntry.Text) ? (decimal?)null : decimal.Parse(maxPriceEntry.Text);

            // Retrieve the selected category
            string selectedCategory = categoryPicker.SelectedItem?.ToString();
            if (selectedCategory == "Toutes les catégories")
            {
                selectedCategory = null; // Send null to not filter by category
            }

            // Call the filtering method with the parameters
            await ViewModel.LoadFilteredGames(searchTerm, minPrice, maxPrice, selectedCategory);
        }

        private async void OnResetClicked(object sender, EventArgs e)
        {
            // Reset the filter fields
            searchTermEntry.Text = string.Empty;
            minPriceEntry.Text = string.Empty;
            maxPriceEntry.Text = string.Empty;
            categoryPicker.SelectedItem = null; // Reset the Picker selection

            // Load all games without parameters
            await ViewModel.LoadFilteredGames(null, null, null, null);
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
    }
}
