using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

public interface IAuthService
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }

    Task<(bool Success, string? Error)> SignUpAsync(string email, string password, string displayName, UserMode mode, string? shopName = null);
    Task<(bool Success, string? Error)> SignInAsync(string email, string password);
    Task<(bool Success, string? Error)> SignInWithGoogleAsync();
    Task<(bool Success, string? Error)> SignInWithFacebookAsync();
    Task<(bool Success, string? Error)> SignInWithAppleAsync();
    Task SignOutAsync();
    Task<(bool Success, string? Error)> UpdateUserModeAsync(UserMode mode, string? shopName = null);
    Task<User?> GetCurrentUserAsync();
    Task SaveUserAsync(User user);
}
