using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

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

    public SettingsViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Settings";
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
                await Shell.Current.DisplayAlert("Mode Changed",
                    $"You are now in {(IsBarberMode ? "Barber" : "Client")} mode.", "OK");

                // Refresh the main page
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                // Revert the toggle
                IsBarberMode = !IsBarberMode;
                await Shell.Current.DisplayAlert("Error", error ?? "Failed to change mode.", "OK");
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
                    await Shell.Current.DisplayAlert("Error", error ?? "Failed to update shop name.", "OK");
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
        var confirm = await Shell.Current.DisplayAlert(
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
