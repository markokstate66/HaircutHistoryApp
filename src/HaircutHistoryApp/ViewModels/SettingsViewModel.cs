using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IThemeService _themeService;
    private readonly IProfilePictureService _profilePictureService;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private bool _isBarberMode;

    [ObservableProperty]
    private string _shopName = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private int _selectedThemeIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProfilePicture))]
    [NotifyPropertyChangedFor(nameof(HasNoProfilePicture))]
    private string? _profilePictureUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AuthProviderDisplay))]
    private AuthProvider _authProvider;

    [ObservableProperty]
    private bool _canUploadProfilePicture;

    public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureUrl);
    public bool HasNoProfilePicture => string.IsNullOrEmpty(ProfilePictureUrl);

    public string AuthProviderDisplay => AuthProvider switch
    {
        AuthProvider.Google => "Signed in with Google",
        AuthProvider.Facebook => "Signed in with Facebook",
        AuthProvider.Apple => "Signed in with Apple",
        AuthProvider.Device => "Guest account",
        _ => "Email account"
    };

    public List<string> ThemeOptions { get; } = new() { "System Default", "Light", "Dark" };

    public SettingsViewModel(IAuthService authService, IThemeService themeService, IProfilePictureService profilePictureService)
    {
        _authService = authService;
        _themeService = themeService;
        _profilePictureService = profilePictureService;
        Title = "Settings";

        // Load current theme
        SelectedThemeIndex = (int)_themeService.CurrentTheme;
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var theme = (Services.AppTheme)value;
        _themeService.SetTheme(theme);
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            CurrentUser = await _authService.GetCurrentUserAsync();

            if (CurrentUser == null)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            DisplayName = CurrentUser.DisplayName;
            Email = CurrentUser.Email;
            IsBarberMode = CurrentUser.Mode == UserMode.Barber;
            ShopName = CurrentUser.ShopName ?? string.Empty;
            ProfilePictureUrl = CurrentUser.EffectiveProfilePictureUrl;
            AuthProvider = CurrentUser.AuthProvider;
            CanUploadProfilePicture = CurrentUser.CanUploadCustomPicture;
        });
    }

    [RelayCommand]
    private async Task ChangeProfilePictureAsync()
    {
        // For social login users, show info about their provider picture
        if (!CanUploadProfilePicture)
        {
            var providerName = AuthProvider switch
            {
                AuthProvider.Google => "Google",
                AuthProvider.Facebook => "Facebook",
                AuthProvider.Apple => "Apple",
                _ => "your account provider"
            };

            await Shell.Current.DisplayAlertAsync(
                "Profile Picture",
                $"Your profile picture is synced from {providerName}. To change it, update your picture in {providerName}.",
                "OK");
            return;
        }

        // For email users, show options to upload
        var action = await Shell.Current.DisplayActionSheetAsync(
            "Change Profile Picture",
            "Cancel",
            HasProfilePicture ? "Remove Picture" : null,
            "Take Photo",
            "Choose from Gallery");

        if (action == "Cancel" || string.IsNullOrEmpty(action))
            return;

        if (action == "Remove Picture")
        {
            await RemoveProfilePictureAsync();
            return;
        }

        string? localPath = null;

        if (action == "Take Photo")
        {
            localPath = await _profilePictureService.TakeProfilePictureAsync();
        }
        else if (action == "Choose from Gallery")
        {
            localPath = await _profilePictureService.PickProfilePictureAsync();
        }

        if (string.IsNullOrEmpty(localPath))
            return;

        await UploadProfilePictureAsync(localPath);
    }

    private async Task UploadProfilePictureAsync(string localPath)
    {
        await ExecuteAsync(async () =>
        {
            if (CurrentUser == null)
                return;

            var cloudUrl = await _profilePictureService.UploadProfilePictureAsync(localPath, CurrentUser.Id);

            if (!string.IsNullOrEmpty(cloudUrl))
            {
                // Delete old picture if exists
                if (!string.IsNullOrEmpty(CurrentUser.CustomProfilePictureUrl))
                {
                    await _profilePictureService.DeleteProfilePictureAsync(CurrentUser.Id, CurrentUser.CustomProfilePictureUrl);
                }

                CurrentUser.CustomProfilePictureUrl = cloudUrl;
                CurrentUser.UpdatedAt = DateTime.UtcNow;
                await _authService.SaveUserAsync(CurrentUser);

                ProfilePictureUrl = CurrentUser.EffectiveProfilePictureUrl;

                await Shell.Current.DisplayAlertAsync("Success", "Profile picture updated!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "Failed to upload picture. Please try again.", "OK");
            }
        });
    }

    private async Task RemoveProfilePictureAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.CustomProfilePictureUrl))
                return;

            var confirm = await Shell.Current.DisplayAlertAsync(
                "Remove Picture",
                "Are you sure you want to remove your profile picture?",
                "Remove", "Cancel");

            if (!confirm)
                return;

            await _profilePictureService.DeleteProfilePictureAsync(CurrentUser.Id, CurrentUser.CustomProfilePictureUrl);

            CurrentUser.CustomProfilePictureUrl = null;
            CurrentUser.UpdatedAt = DateTime.UtcNow;
            await _authService.SaveUserAsync(CurrentUser);

            ProfilePictureUrl = CurrentUser.EffectiveProfilePictureUrl;
        });
    }

    [RelayCommand]
    private async Task ToggleModeAsync()
    {
        var newMode = IsBarberMode ? UserMode.Barber : UserMode.Client;

        string? newShopName = null;
        if (newMode == UserMode.Barber && string.IsNullOrEmpty(ShopName))
        {
            newShopName = await Shell.Current.DisplayPromptAsync(
                "Shop Name",
                "Enter your shop/salon name (optional):",
                keyboard: Keyboard.Text);

            if (newShopName != null)
            {
                ShopName = newShopName;
            }
        }

        await ExecuteAsync(async () =>
        {
            var (success, error) = await _authService.UpdateUserModeAsync(
                newMode, IsBarberMode ? ShopName : null);

            if (success)
            {
                await Shell.Current.DisplayAlertAsync("Mode Changed",
                    $"You are now in {(IsBarberMode ? "Barber" : "Client")} mode.", "OK");

                // Refresh the main page
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                // Revert the toggle
                IsBarberMode = !IsBarberMode;
                await Shell.Current.DisplayAlertAsync("Error", error ?? "Failed to change mode.", "OK");
            }
        });
    }

    [RelayCommand]
    private async Task UpdateShopNameAsync()
    {
        if (!IsBarberMode)
            return;

        var newName = await Shell.Current.DisplayPromptAsync(
            "Shop Name",
            "Enter your shop/salon name:",
            initialValue: ShopName,
            keyboard: Keyboard.Text);

        if (newName != null && newName != ShopName)
        {
            await ExecuteAsync(async () =>
            {
                var (success, error) = await _authService.UpdateUserModeAsync(UserMode.Barber, newName);

                if (success)
                {
                    ShopName = newName;
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", error ?? "Failed to update shop name.", "OK");
                }
            });
        }
    }

    [RelayCommand]
    private async Task ViewAchievementsAsync()
    {
        await Shell.Current.GoToAsync("achievements");
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Sign Out",
            "Are you sure you want to sign out?",
            "Sign Out", "Cancel");

        if (confirm)
        {
            await _authService.SignOutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
