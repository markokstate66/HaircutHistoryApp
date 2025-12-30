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

    [ObservableProperty]
    private string? _emailError;

    [ObservableProperty]
    private string? _passwordError;

    [ObservableProperty]
    private string? _confirmPasswordError;

    [ObservableProperty]
    private string? _displayNameError;

    [ObservableProperty]
    private string? _shopNameError;

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Create Account";
    }

    private bool ValidateInput()
    {
        // Clear all errors
        EmailError = null;
        PasswordError = null;
        ConfirmPasswordError = null;
        DisplayNameError = null;
        ShopNameError = null;

        var isValid = true;

        // Validate email
        var emailResult = InputValidator.ValidateEmail(Email);
        if (!emailResult.IsValid)
        {
            EmailError = emailResult.Error;
            isValid = false;
        }

        // Validate display name
        var displayNameResult = InputValidator.ValidateDisplayName(DisplayName);
        if (!displayNameResult.IsValid)
        {
            DisplayNameError = displayNameResult.Error;
            isValid = false;
        }

        // Validate password strength
        var passwordResult = InputValidator.ValidatePassword(Password);
        if (!passwordResult.IsValid)
        {
            PasswordError = passwordResult.Error;
            isValid = false;
        }

        // Validate passwords match
        var matchResult = InputValidator.ValidatePasswordsMatch(Password, ConfirmPassword);
        if (!matchResult.IsValid)
        {
            ConfirmPasswordError = matchResult.Error;
            isValid = false;
        }

        // Validate shop name if barber mode
        if (IsBarberMode)
        {
            var shopNameResult = InputValidator.ValidateShopName(ShopName);
            if (!shopNameResult.IsValid)
            {
                ShopNameError = shopNameResult.Error;
                isValid = false;
            }
        }

        return isValid;
    }

    [RelayCommand]
    private async Task SignUpAsync()
    {
        if (!ValidateInput())
            return;

        await ExecuteAsync(async () =>
        {
            var mode = IsBarberMode ? UserMode.Barber : UserMode.Client;
            var sanitizedEmail = InputValidator.Sanitize(Email, 254);
            var sanitizedDisplayName = InputValidator.Sanitize(DisplayName, 50);
            var sanitizedShopName = IsBarberMode ? InputValidator.Sanitize(ShopName, 100) : null;

            var (success, error) = await _authService.SignUpAsync(
                sanitizedEmail, Password, sanitizedDisplayName, mode, sanitizedShopName);

            if (success)
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Registration Failed", error ?? "Unable to create account.", "OK");
            }
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
