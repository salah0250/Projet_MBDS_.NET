using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Pages;
public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel(); 
    }
}
