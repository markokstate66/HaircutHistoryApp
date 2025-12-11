using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private bool _isBarberMode;

    [ObservableProperty]
    private string _shopName = string.Empty;

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Create Account";
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(DisplayName))
        {
            await Shell.Current.DisplayAlert("Validation", "Please fill in all required fields.", "OK");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await Shell.Current.DisplayAlert("Validation", "Passwords do not match.", "OK");
            return;
        }

        if (Password.Length < 6)
        {
            await Shell.Current.DisplayAlert("Validation", "Password must be at least 6 characters.", "OK");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var mode = IsBarberMode ? UserMode.Barber : UserMode.Client;
            var (success, error) = await _authService.SignUpAsync(
                Email, Password, DisplayName, mode,
                IsBarberMode ? ShopName : null);

            if (success)
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                await Shell.Current.DisplayAlert("Registration Failed", error ?? "Unable to create account.", "OK");
            }
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
