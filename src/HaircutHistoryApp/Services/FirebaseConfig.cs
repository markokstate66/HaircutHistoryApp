namespace HaircutHistoryApp.Services;

/// <summary>
/// Firebase Authentication configuration.
/// </summary>
public static class FirebaseConfig
{
    /// <summary>
    /// Firebase project ID.
    /// </summary>
    public const string ProjectId = "haircut-history-361f3";

    /// <summary>
    /// Firebase Web API Key.
    /// </summary>
    public const string ApiKey = "AIzaSyBbTBuTCvcSnrdAZ5NGt5aQsngGvqptqa0";

    /// <summary>
    /// Azure Functions API base URL.
    /// </summary>
    public const string ApiBaseUrl = "https://haircuthistory-api.azurewebsites.net/api";

    /// <summary>
    /// Local development API URL.
    /// </summary>
    public const string LocalApiBaseUrl = "http://localhost:7071/api";

    /// <summary>
    /// Whether to use local development API.
    /// </summary>
#if DEBUG
    public const bool UseLocalApi = false; // Set to true for local debugging
#else
    public const bool UseLocalApi = false;
#endif

    /// <summary>
    /// Gets the effective API base URL.
    /// </summary>
    public static string EffectiveApiBaseUrl => UseLocalApi ? LocalApiBaseUrl : ApiBaseUrl;

    // Firebase Auth REST API endpoints
    public static class Endpoints
    {
        private const string IdentityToolkitBase = "https://identitytoolkit.googleapis.com/v1";
        private const string SecureTokenBase = "https://securetoken.googleapis.com/v1";

        /// <summary>
        /// Sign in with email/password.
        /// </summary>
        public static string SignInWithPassword => $"{IdentityToolkitBase}/accounts:signInWithPassword?key={ApiKey}";

        /// <summary>
        /// Sign up with email/password.
        /// </summary>
        public static string SignUp => $"{IdentityToolkitBase}/accounts:signUp?key={ApiKey}";

        /// <summary>
        /// Sign in with OAuth provider (Google, Apple, etc.).
        /// </summary>
        public static string SignInWithIdp => $"{IdentityToolkitBase}/accounts:signInWithIdp?key={ApiKey}";

        /// <summary>
        /// Get user data.
        /// </summary>
        public static string GetAccountInfo => $"{IdentityToolkitBase}/accounts:lookup?key={ApiKey}";

        /// <summary>
        /// Update user profile.
        /// </summary>
        public static string UpdateProfile => $"{IdentityToolkitBase}/accounts:update?key={ApiKey}";

        /// <summary>
        /// Send password reset email.
        /// </summary>
        public static string SendPasswordReset => $"{IdentityToolkitBase}/accounts:sendOobCode?key={ApiKey}";

        /// <summary>
        /// Delete account.
        /// </summary>
        public static string DeleteAccount => $"{IdentityToolkitBase}/accounts:delete?key={ApiKey}";

        /// <summary>
        /// Refresh ID token.
        /// </summary>
        public static string RefreshToken => $"{SecureTokenBase}/token?key={ApiKey}";
    }

    // Google OAuth Client IDs from Firebase project
    public static class Google
    {
        // Web client ID - used for requesting ID tokens
        public const string WebClientId = "517009667256-7vs9vo255iga71ks9a1m9fam1ojkbtue.apps.googleusercontent.com";
    }

    // Apple Sign In configuration
    public static class Apple
    {
        /// <summary>
        /// Apple Services ID for Sign in with Apple.
        /// Get from: Apple Developer Console > Identifiers > Services IDs
        /// TODO: Replace with your actual Apple Services ID
        /// </summary>
        public const string ServicesId = "com.summittechnologygroup.haircuthistory";
    }
}
