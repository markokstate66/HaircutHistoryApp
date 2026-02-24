using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IProfilePictureService _profilePictureService;
    private readonly ISubscriptionService _subscriptionService;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProfilePicture))]
    [NotifyPropertyChangedFor(nameof(HasNoProfilePicture))]
    private string? _profilePictureUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AuthProviderDisplay))]
    private AuthProvider _authProvider;

    [ObservableProperty]
    private bool _canUploadProfilePicture;

    [ObservableProperty]
    private bool _isOfflineMode;

#if DEBUG
    [ObservableProperty]
    private bool _isDebugPremium;
#endif

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

    public SettingsViewModel(IAuthService authService, IProfilePictureService profilePictureService, ISubscriptionService subscriptionService)
    {
        _authService = authService;
        _profilePictureService = profilePictureService;
        _subscriptionService = subscriptionService;
        Title = "Settings";
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        await ExecuteAsync(async () =>
        {
            IsOfflineMode = Preferences.Get("OfflineMode", false);

#if DEBUG
            IsDebugPremium = _subscriptionService.IsPremium;
#endif

            CurrentUser = await _authService.GetCurrentUserAsync();

            if (CurrentUser == null && !IsOfflineMode)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            if (CurrentUser != null)
            {
                DisplayName = CurrentUser.DisplayName;
                Email = CurrentUser.Email;
                ProfilePictureUrl = CurrentUser.EffectiveProfilePictureUrl;
                AuthProvider = CurrentUser.AuthProvider;
                CanUploadProfilePicture = CurrentUser.CanUploadCustomPicture;
            }
            else
            {
                DisplayName = "Local User";
                Email = "Data stored on this device only";
                AuthProvider = AuthProvider.Device;
            }
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
    private async Task ViewAchievementsAsync()
    {
        await Shell.Current.GoToAsync("achievements");
    }

    [RelayCommand]
    private async Task GoToThemesAsync()
    {
        await Shell.Current.GoToAsync("themeSelection");
    }

    [RelayCommand]
    private async Task RateAppAsync()
    {
        try
        {
#if ANDROID
            var uri = new Uri("market://details?id=com.stg.haircuthistory");
#elif IOS
            var uri = new Uri("itms-apps://itunes.apple.com/app/id123456789"); // Replace with actual App Store ID
#else
            var uri = new Uri("https://play.google.com/store/apps/details?id=com.stg.haircuthistory");
#endif
            await Launcher.OpenAsync(uri);
        }
        catch
        {
            // Fallback to web URL
            await Launcher.OpenAsync("https://play.google.com/store/apps/details?id=com.stg.haircuthistory");
        }
    }

    [RelayCommand]
    private async Task ShareAppAsync()
    {
        await Share.RequestAsync(new ShareTextRequest
        {
            Title = "Share Haircut History",
            Text = "Check out Haircut History - the app that helps you remember your perfect haircut!\n\nhttps://play.google.com/store/apps/details?id=com.stg.haircuthistory"
        });
    }

    [RelayCommand]
    private async Task ContactSupportAsync()
    {
        try
        {
            var deviceInfo = $"\n\n---\nDevice: {DeviceInfo.Manufacturer} {DeviceInfo.Model}\nOS: {DeviceInfo.Platform} {DeviceInfo.VersionString}\nApp Version: 1.0.0";
            var body = $"Hi, I need help with...\n{deviceInfo}";

            await Microsoft.Maui.ApplicationModel.Communication.Email.Default.ComposeAsync("Haircut History Support", body, "support@haircuthistory.com");
        }
        catch
        {
            await Shell.Current.DisplayAlertAsync("Email Not Available", "Please email us at support@haircuthistory.com", "OK");
        }
    }

    [RelayCommand]
    private async Task OpenPrivacyPolicyAsync()
    {
        await Launcher.OpenAsync("https://haircuthistory.com/privacy");
    }

    [RelayCommand]
    private async Task OpenTermsOfServiceAsync()
    {
        await Launcher.OpenAsync("https://haircuthistory.com/terms");
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        // Clear offline mode and go to login
        Preferences.Remove("OfflineMode");
        await Shell.Current.GoToAsync("//login");
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
            Preferences.Remove("OfflineMode");
            await _authService.SignOutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

#if DEBUG
    [RelayCommand]
    private async Task ToggleDebugPremiumAsync()
    {
        IsDebugPremium = !IsDebugPremium;
        await _subscriptionService.SetDebugPremiumAsync(IsDebugPremium);
        await Shell.Current.DisplayAlertAsync(
            "Debug Mode",
            $"Premium mode {(IsDebugPremium ? "enabled" : "disabled")}. Restart may be needed for some features.",
            "OK");
    }
#endif
}
