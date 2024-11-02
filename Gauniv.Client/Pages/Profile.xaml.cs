using Gauniv.Client.ViewModel;
using Microsoft.UI.Xaml.Controls;

namespace Gauniv.Client.Pages;

public partial class Profile : ContentPage
{
    public Profile(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Initial animations
        ProfileFrame.Opacity = 0;
        InfoCard.TranslationY = 50;
        InfoCard.Opacity = 0;

        // Animate profile image
        await ProfileFrame.FadeTo(1, 800, Easing.CubicOut);
        await ProfileFrame.RotateTo(360, 800, Easing.CubicOut);

        // Animate info card
        await Task.WhenAll(
            InfoCard.TranslateTo(0, 0, 500, Easing.CubicOut),
            InfoCard.FadeTo(1, 500, Easing.CubicOut)
        );

        // Load profile data
        if (BindingContext is ProfileViewModel viewModel)
        {
            await viewModel.OnAppearing();
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        // Ensure profile frame maintains perfect circle shape
        if (width > 0)
        {
            ProfileFrame.WidthRequest = Math.Min(width * 0.4, 160);
            ProfileFrame.HeightRequest = ProfileFrame.WidthRequest;
            ProfileFrame.CornerRadius = (float)(ProfileFrame.WidthRequest / 2);
        }
    }
}
