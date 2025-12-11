using HaircutHistoryApp.Models;
using Newtonsoft.Json;

namespace HaircutHistoryApp.Services;

public class PlayFabAuthService : IAuthService
{
    private readonly IPlayFabService _playFabService;
    private User? _currentUser;

    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _playFabService.IsLoggedIn && _currentUser != null;

    public PlayFabAuthService(IPlayFabService playFabService)
    {
        _playFabService = playFabService;
    }

    public async Task<(bool Success, string? Error)> SignUpAsync(string email, string password, string displayName, UserMode mode, string? shopName = null)
    {
        var (success, error) = await _playFabService.RegisterAsync(email, password, displayName);

        if (!success)
            return (false, error);

        _currentUser = new User
        {
            Id = _playFabService.PlayFabId ?? Guid.NewGuid().ToString(),
            Email = email,
            DisplayName = displayName,
            Mode = mode,
            ShopName = shopName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Save user profile to PlayFab
        await SaveUserAsync(_currentUser);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> SignInAsync(string email, string password)
    {
        var (success, error) = await _playFabService.LoginAsync(email, password);

        if (!success)
            return (false, error);

        // Load user profile from PlayFab
        _currentUser = await LoadUserProfileAsync();

        if (_currentUser == null)
        {
            // Create a basic profile if none exists
            _currentUser = new User
            {
                Id = _playFabService.PlayFabId ?? Guid.NewGuid().ToString(),
                Email = email,
                DisplayName = email.Split('@')[0],
                Mode = UserMode.Client,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await SaveUserAsync(_currentUser);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> SignInWithGoogleAsync()
    {
        try
        {
            // Use WebAuthenticator to get Google OAuth token
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri($"https://accounts.google.com/o/oauth2/v2/auth?client_id={SocialAuthConfig.GoogleClientId}&redirect_uri={SocialAuthConfig.RedirectUri}&response_type=token&scope=email%20profile"),
                new Uri(SocialAuthConfig.RedirectUri));

            var accessToken = authResult.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
                return (false, "Failed to get Google access token");

            var (success, error) = await _playFabService.LoginWithGoogleAsync(accessToken);

            if (!success)
                return (false, error);

            _currentUser = await LoadUserProfileAsync();

            if (_currentUser == null)
            {
                // Create profile for new Google user
                _currentUser = new User
                {
                    Id = _playFabService.PlayFabId ?? Guid.NewGuid().ToString(),
                    Email = "google-user",
                    DisplayName = "Google User",
                    Mode = UserMode.Client,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await SaveUserAsync(_currentUser);
            }

            return (true, null);
        }
        catch (TaskCanceledException)
        {
            return (false, "Google sign-in was cancelled");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SignInWithFacebookAsync()
    {
        try
        {
            // Use WebAuthenticator to get Facebook OAuth token
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri($"https://www.facebook.com/v18.0/dialog/oauth?client_id={SocialAuthConfig.FacebookAppId}&redirect_uri={SocialAuthConfig.RedirectUri}&response_type=token&scope=email,public_profile"),
                new Uri(SocialAuthConfig.RedirectUri));

            var accessToken = authResult.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
                return (false, "Failed to get Facebook access token");

            var (success, error) = await _playFabService.LoginWithFacebookAsync(accessToken);

            if (!success)
                return (false, error);

            _currentUser = await LoadUserProfileAsync();

            if (_currentUser == null)
            {
                _currentUser = new User
                {
                    Id = _playFabService.PlayFabId ?? Guid.NewGuid().ToString(),
                    Email = "facebook-user",
                    DisplayName = "Facebook User",
                    Mode = UserMode.Client,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await SaveUserAsync(_currentUser);
            }

            return (true, null);
        }
        catch (TaskCanceledException)
        {
            return (false, "Facebook sign-in was cancelled");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SignInWithAppleAsync()
    {
        try
        {
#if IOS || MACCATALYST
            // Use Apple Sign In (iOS native)
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = new Uri("https://appleid.apple.com/auth/authorize"),
                    CallbackUrl = new Uri(SocialAuthConfig.RedirectUri),
                    PrefersEphemeralWebBrowserSession = true
                });

            var identityToken = authResult.IdToken;

            if (string.IsNullOrEmpty(identityToken))
                return (false, "Failed to get Apple identity token");

            var (success, error) = await _playFabService.LoginWithAppleAsync(identityToken);

            if (!success)
                return (false, error);

            _currentUser = await LoadUserProfileAsync();

            if (_currentUser == null)
            {
                _currentUser = new User
                {
                    Id = _playFabService.PlayFabId ?? Guid.NewGuid().ToString(),
                    Email = "apple-user",
                    DisplayName = "Apple User",
                    Mode = UserMode.Client,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await SaveUserAsync(_currentUser);
            }

            return (true, null);
#else
            return (false, "Apple Sign In is only available on iOS");
#endif
        }
        catch (TaskCanceledException)
        {
            return (false, "Apple sign-in was cancelled");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task SignOutAsync()
    {
        await _playFabService.LogoutAsync();
        _currentUser = null;
    }

    public async Task<(bool Success, string? Error)> UpdateUserModeAsync(UserMode mode, string? shopName = null)
    {
        if (_currentUser == null)
            return (false, "Not authenticated");

        _currentUser.Mode = mode;
        _currentUser.ShopName = shopName;
        _currentUser.UpdatedAt = DateTime.UtcNow;

        await SaveUserAsync(_currentUser);

        return (true, null);
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        if (!_playFabService.IsLoggedIn)
        {
            // Try device login for returning users
            var (success, _) = await _playFabService.LoginWithDeviceAsync();
            if (!success)
                return null;
        }

        _currentUser = await LoadUserProfileAsync();
        return _currentUser;
    }

    public async Task SaveUserAsync(User user)
    {
        var json = JsonConvert.SerializeObject(user);
        await _playFabService.SavePlayerDataAsync("user_profile", json);

        if (_currentUser?.Id == user.Id)
        {
            _currentUser = user;
        }
    }

    private async Task<User?> LoadUserProfileAsync()
    {
        var json = await _playFabService.GetPlayerDataAsync("user_profile");

        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<User>(json);
        }
        catch
        {
            return null;
        }
    }
}
