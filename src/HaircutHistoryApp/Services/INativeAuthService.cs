namespace HaircutHistoryApp.Services;

/// <summary>
/// Platform-specific native authentication service.
/// Provides native sign-in UI for Google (Android) and Apple (iOS).
/// </summary>
public interface INativeAuthService
{
    /// <summary>
    /// Attempts native sign-in using the platform's native UI.
    /// Returns user info if successful.
    /// </summary>
    Task<NativeAuthResult> SignInAsync();

    /// <summary>
    /// Signs out from the native provider.
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Checks if user is already signed in (for silent auth).
    /// </summary>
    Task<NativeAuthResult?> TrySilentSignInAsync();
}

/// <summary>
/// Result from native authentication.
/// </summary>
public class NativeAuthResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    /// <summary>
    /// User's email from the native provider.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User's display name from the native provider.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// User's unique ID from the native provider.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// ID token for backend verification (JWT).
    /// </summary>
    public string? IdToken { get; set; }

    /// <summary>
    /// Profile picture URL if available.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// The authentication provider used.
    /// </summary>
    public string? Provider { get; set; }
}
