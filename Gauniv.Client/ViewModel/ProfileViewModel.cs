using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.DTOs;
using Gauniv.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly IProfileService _profileService;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string id;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string firstName;

        [ObservableProperty]
        private string lastName;

        [ObservableProperty]
        private string phoneNumber;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isEditMode;

        // Backup properties for cancellation
        private string _backupEmail;
        private string _backupFirstName;
        private string _backupLastName;
        private string _backupPhoneNumber;

        [ObservableProperty]
        private ObservableCollection<UserGameDto> recentGames;

        public bool IsNotEditMode => !IsEditMode;

        public ProfileViewModel(IProfileService profileService, AuthService authService)
        {
            _profileService = profileService;
            _authService = authService;
        }

        partial void OnIsEditModeChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotEditMode));
        }

        [RelayCommand]
        private void ToggleEditMode()
        {
            if (!IsEditMode)
            {
                // Entering edit mode - backup current values
                _backupEmail = Email;
                _backupFirstName = FirstName;
                _backupLastName = LastName;
                _backupPhoneNumber = PhoneNumber;
                IsEditMode = true;
            }
            else
            {
                // Exiting edit mode - restore backup values
                Email = _backupEmail;
                FirstName = _backupFirstName;
                LastName = _backupLastName;
                PhoneNumber = _backupPhoneNumber;
                IsEditMode = false;
            }
        }

        [RelayCommand]
        private async Task LoadProfileAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var profile = await _profileService.GetUserProfileAsync();
                Id = profile.Id;
                Email = profile.Email;
                FirstName = profile.FirstName;
                LastName = profile.LastName;
                PhoneNumber = profile.PhoneNumber;
                RecentGames = new ObservableCollection<UserGameDto>(profile.UserGames);

                // Reset edit mode when loading new data
                IsEditMode = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UpdateProfileAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var updatedProfile = await _profileService.UpdateProfileAsync(Email, FirstName, LastName, PhoneNumber);
                Id = updatedProfile.Id;
                Email = updatedProfile.Email;
                FirstName = updatedProfile.FirstName;
                LastName = updatedProfile.LastName;
                PhoneNumber = updatedProfile.PhoneNumber;

                // Update backup values after successful update
                _backupEmail = Email;
                _backupFirstName = FirstName;
                _backupLastName = LastName;
                _backupPhoneNumber = PhoneNumber;

                // Exit edit mode after successful update
                IsEditMode = false;

                await Application.Current.MainPage.DisplayAlert("Success", "Profile updated successfully!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to update profile. Please try again.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ChangeProfilePhotoAsync()
        {
            // Implement the logic to change the profile photo
            await Application.Current.MainPage.DisplayAlert("Change Photo", "Change profile photo logic goes here.", "OK");
        }

        public async Task OnAppearing()
        {
            await LoadProfileAsync();
        }
    }
}
