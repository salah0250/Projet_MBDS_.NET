using Gauniv.Client.Services;
using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Pages
{
    public partial class Index : ContentPage
    {
        public Index(GameService gameService)
        {
            InitializeComponent(); // This calls the auto-generated method from XAML
            BindingContext = new IndexViewModel(gameService); // Set the ViewModel manually
        }
    }
}
