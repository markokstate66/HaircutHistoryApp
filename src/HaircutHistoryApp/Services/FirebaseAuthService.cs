using System.Net.Http.Json;
using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Authentication service using Firebase Authentication.
/// </summary>
public class FirebaseAuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IApiService _apiService;
    private readonly ILogService _logService;
    private readonly INativeAuthService _nativeAuthService;

    private User? _currentUser;
    private string? _idToken;
    private string? _refreshToken;
    private DateTime _tokenExpiry;

    // SecureStorage keys
    private const string RefreshTokenKey = "firebase_refresh_token";
    private const string UserIdKey = "firebase_user_id";
    private const string UserEmailKey = "firebase_user_email";
    private const string UserDisplayNameKey = "firebase_user_display_name";
    private const string UserPhotoUrlKey = "firebase_user_photo_url";
    private const string AuthProviderKey = "firebase_auth_provider";

    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_idToken);

    public FirebaseAuthService(
        IHttpClientFactory httpClientFactory,
        IApiService apiService,
        ILogService logService,
        INativeAuthService nativeAuthService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiService = apiService;
        _logService = logService;
        _nativeAuthService = nativeAuthService;
    }

    /// <summary>
    /// Gets the current ID token, refreshing if needed.
    /// </summary>
    public async Task<string?> GetIdTokenAsync()
    {
        if (string.IsNullOrEmpty(_idToken))
            return null;

        // Refresh if within 5 minutes of expiry
        if (DateTime.UtcNow >= _tokenExpiry.AddMinutes(-5))
        {
            await RefreshTokenAsync();
        }

        return _idToken;
    }

    public async Task<(bool Success, string? Error)> SignUpAsync(
        string email, string password, string displayName, UserMode mode, string? shopName = null)
    {
        try
        {
            var request = new FirebaseSignUpRequest
            {
                Email = email,
                Password = password,
                DisplayName = displayName
            };

            var response = await _httpClient.PostAsJsonAsync(
                FirebaseConfig.Endpoints.SignUp, request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<FirebaseErrorResponse>();
                var errorMessage = ParseFirebaseError(error?.Error?.Message);
                return (false, errorMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>();
            if (result == null)
                return (false, "Failed to parse Firebase response");

            // Update display name if not set in initial response
            if (string.IsNullOrEmpty(result.DisplayName) && !string.IsNullOrEmpty(displayName))
            {
                await UpdateProfileAsync(result.IdToken, displayName, null);
            }

            await ProcessAuthResponse(result, AuthProvider.Email, displayName);

            _logService.Info("FirebaseAuth", $"User signed up: {email}");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logService.Error("FirebaseAuth", "Sign-up failed", ex);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SignInAsync(string email, string password)
    {
        try
        {
            var request = new FirebaseSignInRequest
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync(
                FirebaseConfig.Endpoints.SignInWithPassword, request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<FirebaseErrorResponse>();
                var errorMessage = ParseFirebaseError(error?.Error?.Message);
                return (false, errorMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>();
            if (result == null)
                return (false, "Failed to parse Firebase response");

            await ProcessAuthResponse(result, AuthProvider.Email, null);

            _logService.Info("FirebaseAuth", $"User signed in: {email}");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logService.Error("FirebaseAuth", "Sign-in failed", ex);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SignInWithGoogleAsync()
    {
        try
        {
            // Use native Google Sign-In to get OAuth token
            var nativeResult = await _nativeAuthService.SignInAsync();

            if (!nativeResult.Success || string.IsNullOrEmpty(nativeResult.IdToken))
            {
                return (false, nativeResult.Error ?? "Google sign-in failed");
            }

            // Exchange Google token with Firebase
            return await SignInWithIdpAsync(
                nativeResult.IdToken,
                "google.com",
                AuthProvider.Google,
                nativeResult.DisplayName,
                nativeResult.PhotoUrl);
        }
        catch (Exception ex)
        {
            _logService.Error("FirebaseAuth", "Google sign-in failed", ex);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SignInWithFacebookAsync()
    {
        // Facebook sign-in not implemented yet
        return (false, "Facebook sign-in is not available yet");
    }

    public async Task<(bool Success, string? Error)> SignInWithAppleAsync()
    {
        try
        {
            // Apple Sign In implementation depends on platform
            // For iOS, use AuthenticationServices framework
            // For now, return not implemented
            return (false, "Apple sign-in is not available yet");
        }
        catch (Exception ex)
        {
            _logService.Error("FirebaseAuth", "Apple sign-in failed", ex);
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Signs in with an identity provider (Google, Apple, etc.) by exchanging OAuth token.
    /// </summary>
    private async Task<(bool Success, string? Error)> SignInWithIdpAsync(
        string oauthToken,
        string providerId,
        AuthProvider authProvider,
        string? displayName,
        string? photoUrl)
    {
        try
        {
            var request = new FirebaseSignInWithIdpRequest
            {
                PostBody = $"id_token={oauthToken}&providerId={providerId}",
                RequestUri = "http://localhost",
                ReturnIdpCredential = true,
                ReturnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                FirebaseConfig.Endpoints.SignInWithIdp, request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<FirebaseErrorResponse>();
                var errorMessage = ParseFirebaseError(error?.Error?.Message);
                return (false, errorMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>();
            if (result == null)
                return (false, "Failed to parse Firebase response");

            // Use display name and photo from OAuth provider if available
            var finalDisplayName = result.DisplayName ?? displayName ?? result.FullName;
            var finalPhotoUrl = result.PhotoUrl ?? photoUrl;

            await ProcessAuthResponse(result, authProvider, finalDisplayName, finalPhotoUrl);

            _logService.Info("FirebaseAuth", $"User signed in with {providerId}: {result.Email}");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logService.Error("FirebaseAuth", $"IdP sign-in failed: {providerId}", ex);
            return (false, ex.Message);
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            // Sign out from native provider
            await _nativeAuthService.SignOutAsync();

            // Clear tokens
            _idToken = null;
            _refreshToken = null;
            _currentUser = null;
            _tokenExpiry = DateTime.MinValue;

            // Clear stored data
            SecureStorage.Remove(RefreshTokenKey);
            Preferences.Remove(UserIdKey);
            Preferences.Remove(UserEmailKey);
            Preferences.Remove(UserDisplayNameKey);
            Preferences.Remove(UserPhotoUrlKey);
            Preferences.Remove(AuthProviderKey);
            Preferences.Remove("OfflineMode");

            // Clear API token
            _apiService.SetAccessToken(null);

            _logService.Info("FirebaseAuth", "User signed out");
        }
        catch (Exception ex)
        {
            _logService.Error("FirebaseAuth", "Sign-out error", ex);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateUserModeAsync(UserMode mode, string? shopName = null)
    {
        // User mode is handled in the backend
        return (true, null);
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        // Try to restore session from stored credentials
        var restored = await TryRestoreSessionAsync();
        if (restored)
            return _currentUser;

        return null;
    }

    public async Task SaveUserAsync(User user)
    {
        // Update user in backend
        await _apiService.CreateOrUpdateUserAsync(user.DisplayName);
        _currentUser = user;
    }

    /// <summary>
    /// Attempts to restore session from stored refresh token.
    /// </summary>
    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            // First, try silent sign-in with native provider (Google)
            var nativeResult = await _nativeAuthService.TrySilentSignInAsync();
            if (nativeResult?.Success == true && !string.IsNullOrEmpty(nativeResult.IdToken))
            {
                _logService.Info("FirebaseAuth", "Attempting silent sign-in with native provider");

                var (success, _) = await SignInWithIdpAsync(
                    nativeResult.IdToken,
                    "google.com",
                    AuthProvider.Google,
                    nativeResult.DisplayName,
                    nativeResult.PhotoUrl);

                if (success)
                {
                    _logService.Info("FirebaseAuth", "Silent sign-in successful");
                    return true;
                }
            }

            // Try to restore from stored refresh token
            var storedRefreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(storedRefreshToken))
                return false;

            _refreshToken = storedRefreshToken;
            var refreshed = await RefreshTokenAsync();

            if (refreshed)
            {
                // Restore user from preferences
                var userId = Preferences.Get(UserIdKey, "");
                var email = Preferences.Get(UserEmailKey, "");
                var displayName = Preferences.Get(UserDisplayNameKey, "User");
                var photoUrl = Preferences.Get(UserPhotoUrlKey, "");
                var providerStr = Preferences.Get(AuthProviderKey, "Email");

                if (!string.IsNullOrEmpty(userId))
                {
                    var authProvider = Enum.TryParse<AuthProvider>(providerStr, out var p) ? p : AuthProvider.Email;

                    _currentUser = new User
                    {
                        Id = userId,
                        Email = email,
                        DisplayName = displayName,
                        ProfilePictureUrl = photoUrl,
                        Mode = UserMode.Client,
                        AuthProvider = authProvider,
                        SubscriptionTier = SubscriptionTier.Free,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _apiService.SetAccessToken(_idToken);

                    // Sync with backend
                    try
                    {
                        var userResponse = await _apiService.GetCurrentUserAsync();
                        if (userResponse.Success && userResponse.Data != null)
                        {
                            _currentUser = ConvertToLocalUser(userResponse.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Warning("FirebaseAuth", $"Could not sync with backend: {ex.Message}");
                    }

                    _logService.Info("FirebaseAuth", $"Session restored for: {email}");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logService.Warning("FirebaseAuth", $"Session restore failed: {ex.Message}");
        }

        return false;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
            return false;

        try
        {
            var request = new FirebaseRefreshRequest
            {
                RefreshToken = _refreshToken
            };

            var response = await _httpClient.PostAsJsonAsync(
                FirebaseConfig.Endpoints.RefreshToken, request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FirebaseRefreshResponse>();
                if (result != null)
                {
                    _idToken = result.IdToken;
                    _refreshToken = result.RefreshToken;
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(int.Parse(result.ExpiresIn));

                    // Update stored refresh token
                    await SecureStorage.SetAsync(RefreshTokenKey, _refreshToken);

                    // Update API service token
                    _apiService.SetAccessToken(_idToken);

                    _logService.Info("FirebaseAuth", "Token refreshed successfully");
                    return true;
                }
            }
            else
            {
                _logService.Warning("FirebaseAuth", "Token refresh failed");
                // Clear invalid refresh token
                SecureStorage.Remove(RefreshTokenKey);
                _refreshToken = null;
            }
        }
        catch (Exception ex)
        {
            _logService.Error("FirebaseAuth", "Token refresh error", ex);
        }

        return false;
    }

    private async Task ProcessAuthResponse(
        FirebaseAuthResponse response,
        AuthProvider authProvider,
        string? displayName,
        string? photoUrl = null)
    {
        _idToken = response.IdToken;
        _refreshToken = response.RefreshToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(int.Parse(response.ExpiresIn));

        var finalDisplayName = displayName ?? response.DisplayName ?? response.Email ?? "User";
        var finalPhotoUrl = photoUrl ?? response.PhotoUrl;

        // Store refresh token securely
        await SecureStorage.SetAsync(RefreshTokenKey, _refreshToken);

        // Store user info in preferences
        Preferences.Set(UserIdKey, response.LocalId);
        Preferences.Set(UserEmailKey, response.Email ?? "");
        Preferences.Set(UserDisplayNameKey, finalDisplayName);
        Preferences.Set(UserPhotoUrlKey, finalPhotoUrl ?? "");
        Preferences.Set(AuthProviderKey, authProvider.ToString());

        // Set token for API calls
        _apiService.SetAccessToken(_idToken);

        // Create local user object
        _currentUser = new User
        {
            Id = response.LocalId,
            Email = response.Email ?? "",
            DisplayName = finalDisplayName,
            ProfilePictureUrl = finalPhotoUrl,
            Mode = UserMode.Client,
            AuthProvider = authProvider,
            SubscriptionTier = SubscriptionTier.Free,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Ensure user exists in backend
        try
        {
            var userResponse = await _apiService.GetCurrentUserAsync();
            if (!userResponse.Success || userResponse.Data == null)
            {
                // Create user in backend
                await _apiService.CreateOrUpdateUserAsync(finalDisplayName);
            }
            else
            {
                // Update local user with backend data
                _currentUser = ConvertToLocalUser(userResponse.Data);
            }
        }
        catch (Exception ex)
        {
            _logService.Warning("FirebaseAuth", $"Could not sync user with backend: {ex.Message}");
        }
    }

    private async Task UpdateProfileAsync(string idToken, string? displayName, string? photoUrl)
    {
        try
        {
            var request = new FirebaseUpdateProfileRequest
            {
                IdToken = idToken,
                DisplayName = displayName,
                PhotoUrl = photoUrl
            };

            await _httpClient.PostAsJsonAsync(FirebaseConfig.Endpoints.UpdateProfile, request);
        }
        catch (Exception ex)
        {
            _logService.Warning("FirebaseAuth", $"Profile update failed: {ex.Message}");
        }
    }

    private User ConvertToLocalUser(Shared.Models.User apiUser)
    {
        return new User
        {
            Id = apiUser.Id,
            Email = apiUser.Email,
            DisplayName = apiUser.DisplayName,
            Mode = UserMode.Client,
            AuthProvider = _currentUser?.AuthProvider ?? AuthProvider.Email,
            SubscriptionTier = apiUser.IsPremium ? SubscriptionTier.Premium : SubscriptionTier.Free,
            SubscriptionExpirationDate = apiUser.PremiumExpiresAt,
            CreatedAt = apiUser.CreatedAt,
            UpdatedAt = apiUser.UpdatedAt
        };
    }

    private string ParseFirebaseError(string? errorCode)
    {
        return errorCode switch
        {
            "EMAIL_NOT_FOUND" => "No account found with this email address",
            "INVALID_PASSWORD" => "Incorrect password",
            "INVALID_LOGIN_CREDENTIALS" => "Invalid email or password",
            "USER_DISABLED" => "This account has been disabled",
            "EMAIL_EXISTS" => "An account already exists with this email",
            "OPERATION_NOT_ALLOWED" => "This sign-in method is not enabled",
            "TOO_MANY_ATTEMPTS_TRY_LATER" => "Too many attempts. Please try again later",
            "WEAK_PASSWORD" => "Password should be at least 6 characters",
            "INVALID_EMAIL" => "Invalid email address",
            _ => errorCode ?? "An error occurred during authentication"
        };
    }
}
