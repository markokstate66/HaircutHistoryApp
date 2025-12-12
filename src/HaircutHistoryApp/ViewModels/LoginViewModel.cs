using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _emailError;

    [ObservableProperty]
    private string? _passwordError;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Sign In";
    }

    private bool ValidateInput()
    {
        EmailError = null;
        PasswordError = null;
        var isValid = true;

        // Validate email
        var emailResult = InputValidator.ValidateEmail(Email);
        if (!emailResult.IsValid)
        {
            EmailError = emailResult.Error;
            isValid = false;
        }

        // For login, use less strict password validation (don't enforce strength)
        var passwordResult = InputValidator.ValidateLoginPassword(Password);
        if (!passwordResult.IsValid)
        {
            PasswordError = passwordResult.Error;
            isValid = false;
        }

        return isValid;
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        if (!ValidateInput())
            return;

        await ExecuteAsync(async () =>
        {
            var (success, error) = await _authService.SignInAsync(Email.Trim(), Password);

            if (success)
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                await AlertService.ShowErrorAsync(error ?? "Invalid email or password. Please try again.", "Sign In Failed");
            }
        }, "Signing in...", "Sign in failed");
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("register");
    }

    [RelayCommand]
    private async Task SignInWithGoogleAsync()
    {
        await ExecuteAsync(async () =>
        {
            var (success, error) = await _authService.SignInWithGoogleAsync();

            if (success)
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                await AlertService.ShowErrorAsync(error ?? "Unable to sign in with Google. Please try again.", "Google Sign In");
            }
        }, "Signing in with Google...", "Google sign in failed");
    }

    [RelayCommand]
    private async Task SignInWithFacebookAsync()
    {
        await ExecuteAsync(async () =>
        {
            var (success, error) = await _authService.SignInWithFacebookAsync();

            if (success)
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                await AlertService.ShowErrorAsync(error ?? "Unable to sign in with Facebook. Please try again.", "Facebook Sign In");
            }
        }, "Signing in with Facebook...", "Facebook sign in failed");
    }

    [RelayCommand]
    private async Task SignInWithAppleAsync()
    {
        await ExecuteAsync(async () =>
        {
            var (success, error) = await _authService.SignInWithAppleAsync();

            if (success)
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                await AlertService.ShowErrorAsync(error ?? "Unable to sign in with Apple. Please try again.", "Apple Sign In");
            }
        }, "Signing in with Apple...", "Apple sign in failed");
    }
}
