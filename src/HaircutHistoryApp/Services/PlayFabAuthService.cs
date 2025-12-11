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
