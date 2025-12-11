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

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Sign In";
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter your email and password.", "OK");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var (success, error) = await _authService.SignInAsync(Email, Password);

            if (success)
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                await Shell.Current.DisplayAlert("Sign In Failed", error ?? "Unable to sign in.", "OK");
            }
        });
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
                await Shell.Current.DisplayAlert("Google Sign In Failed", error ?? "Unable to sign in with Google.", "OK");
            }
        });
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
                await Shell.Current.DisplayAlert("Facebook Sign In Failed", error ?? "Unable to sign in with Facebook.", "OK");
            }
        });
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
                await Shell.Current.DisplayAlert("Apple Sign In Failed", error ?? "Unable to sign in with Apple.", "OK");
            }
        });
    }
}
