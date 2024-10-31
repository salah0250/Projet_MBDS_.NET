using Gauniv.Client.Services;
using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _authService;

        public LoginPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            BindingContext = new LoginViewModel(_authService);

            this.Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, EventArgs e)
        {
            AnimateLoginElements();
        }

        private async void AnimateLoginElements()
        {
            // Configure initial state for all elements
            var elements = new (View Element, double InitialY)[]
            {
        (titleLabel, 20),
        (emailFrame, 20),
        (passwordFrame, 20),
        (rememberMeLayout, 20),
        (signInButton, 20),
        (createAccountLayout, 20)
            };

            // Set initial states
            foreach (var (element, offsetY) in elements)
            {
                element.Opacity = 0;
                element.TranslationY = offsetY;
                element.Scale = 0.95;
            }

            // Initial logo animation
            logoImage.Opacity = 0;
            logoImage.Scale = 0.8;

            // Animate logo first
            await Task.WhenAll(
                logoImage.FadeTo(1, 800, Easing.CubicOut),
                logoImage.ScaleTo(1, 800, Easing.CubicOut)
            );

            // Staggered animations for form elements
            uint duration = 500;
            uint delayBetweenElements = 100;

            for (int i = 0; i < elements.Length; i++)
            {
                var (element, _) = elements[i];

                // Create a smooth animation group for each element
                var animations = new List<Task>
        {
            element.FadeTo(1, duration, Easing.CubicOut),
            element.TranslateTo(0, 0, duration, Easing.CubicOut),
            element.ScaleTo(1, duration, Easing.CubicOut)
        };

                // Run all animations for current element in parallel
                await Task.WhenAll(animations);

                // Small delay before next element
                if (i < elements.Length - 1)
                {
                    await Task.Delay((int)delayBetweenElements);
                }
            }

            // Add subtle hover state for buttons
            signInButton.Pressed += async (s, e) => {
                await signInButton.ScaleTo(0.98, 100, Easing.CubicOut);
            };

            signInButton.Released += async (s, e) => {
                await signInButton.ScaleTo(1, 100, Easing.CubicOut);
            };
        }

    }
}
