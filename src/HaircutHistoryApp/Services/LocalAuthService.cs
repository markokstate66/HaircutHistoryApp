using HaircutHistoryApp.Models;
using Newtonsoft.Json;

namespace HaircutHistoryApp.Services;

public class LocalAuthService : IAuthService
{
    private const string CurrentUserKey = "current_user";
    private const string UsersKey = "users";
    private User? _currentUser;

    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public async Task<(bool Success, string? Error)> SignUpAsync(string email, string password, string displayName, UserMode mode, string? shopName = null)
    {
        try
        {
            var users = await GetUsersAsync();

            if (users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "An account with this email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                DisplayName = displayName,
                Mode = mode,
                ShopName = shopName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            users.Add(user);
            await SaveUsersAsync(users);

            // Store password hash (simplified for demo - use proper hashing in production)
            await SecureStorage.SetAsync($"pwd_{user.Id}", Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(password)));

            _currentUser = user;
            await SaveCurrentUserAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SignInAsync(string email, string password)
    {
        try
        {
            var users = await GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return (false, "No account found with this email.");
            }

            var storedPwd = await SecureStorage.GetAsync($"pwd_{user.Id}");
            var inputPwd = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));

            if (storedPwd != inputPwd)
            {
                return (false, "Incorrect password.");
            }

            _currentUser = user;
            await SaveCurrentUserAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task SignOutAsync()
    {
        _currentUser = null;
        Preferences.Remove(CurrentUserKey);
        await Task.CompletedTask;
    }

    public async Task<(bool Success, string? Error)> UpdateUserModeAsync(UserMode mode, string? shopName = null)
    {
        if (_currentUser == null)
            return (false, "Not authenticated");

        try
        {
            _currentUser.Mode = mode;
            _currentUser.ShopName = shopName;
            _currentUser.UpdatedAt = DateTime.UtcNow;

            var users = await GetUsersAsync();
            var index = users.FindIndex(u => u.Id == _currentUser.Id);
            if (index >= 0)
            {
                users[index] = _currentUser;
                await SaveUsersAsync(users);
            }

            await SaveCurrentUserAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        var json = Preferences.Get(CurrentUserKey, null);
        if (!string.IsNullOrEmpty(json))
        {
            _currentUser = JsonConvert.DeserializeObject<User>(json);
        }

        return await Task.FromResult(_currentUser);
    }

    public async Task SaveUserAsync(User user)
    {
        var users = await GetUsersAsync();
        var index = users.FindIndex(u => u.Id == user.Id);
        if (index >= 0)
        {
            users[index] = user;
        }
        else
        {
            users.Add(user);
        }
        await SaveUsersAsync(users);

        if (_currentUser?.Id == user.Id)
        {
            _currentUser = user;
            await SaveCurrentUserAsync();
        }
    }

    private async Task<List<User>> GetUsersAsync()
    {
        var json = Preferences.Get(UsersKey, null);
        if (string.IsNullOrEmpty(json))
            return new List<User>();

        return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
    }

    private Task SaveUsersAsync(List<User> users)
    {
        var json = JsonConvert.SerializeObject(users);
        Preferences.Set(UsersKey, json);
        return Task.CompletedTask;
    }

    private Task SaveCurrentUserAsync()
    {
        if (_currentUser != null)
        {
            var json = JsonConvert.SerializeObject(_currentUser);
            Preferences.Set(CurrentUserKey, json);
        }
        return Task.CompletedTask;
    }

    public Task<(bool Success, string? Error)> SignInWithGoogleAsync()
    {
        return Task.FromResult<(bool, string?)>((false, "Social login is not available in offline mode. Please use email and password."));
    }

    public Task<(bool Success, string? Error)> SignInWithFacebookAsync()
    {
        return Task.FromResult<(bool, string?)>((false, "Social login is not available in offline mode. Please use email and password."));
    }

    public Task<(bool Success, string? Error)> SignInWithAppleAsync()
    {
        return Task.FromResult<(bool, string?)>((false, "Social login is not available in offline mode. Please use email and password."));
    }
}
