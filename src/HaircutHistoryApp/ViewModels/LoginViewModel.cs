using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private bool _hasAttemptedSignIn;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showRetry;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Sign In";
    }

    /// <summary>
    /// Called when the page appears. Auto-initiates sign-in.
    /// </summary>
    public async Task OnAppearingAsync()
    {
        // Only attempt once per page load
        if (_hasAttemptedSignIn)
            return;

        _hasAttemptedSignIn = true;
        await SignInAsync();
    }

    [RelayCommand]
    private async Task RetrySignInAsync()
    {
        await SignInAsync();
    }

    [RelayCommand]
    private async Task ContinueOfflineAsync()
    {
        // Set offline mode preference
        Preferences.Set("OfflineMode", true);
        await Shell.Current.GoToAsync("//main");
    }

    private async Task SignInAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ShowRetry = false;
        ErrorMessage = null;

        try
        {
            // Check if user previously chose offline mode
            if (Preferences.Get("OfflineMode", false))
            {
                await Shell.Current.GoToAsync("//main");
                return;
            }

            // Try to restore existing session (includes silent sign-in)
            var existingUser = await _authService.GetCurrentUserAsync();
            if (existingUser != null)
            {
                System.Diagnostics.Debug.WriteLine($"Session restored for: {existingUser.Email}");
                await Shell.Current.GoToAsync("//main");
                return;
            }

            // No cached session - trigger Google sign-in through Firebase
            System.Diagnostics.Debug.WriteLine("No cached session, starting Google sign-in...");
            var (success, error) = await _authService.SignInWithGoogleAsync();

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("Google sign-in successful via Firebase");
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                ErrorMessage = error ?? "Unable to sign in. Please try again.";
                ShowRetry = true;
                System.Diagnostics.Debug.WriteLine($"Sign-in failed: {error}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Something went wrong. Please try again.";
            ShowRetry = true;
            System.Diagnostics.Debug.WriteLine($"Sign-in error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
