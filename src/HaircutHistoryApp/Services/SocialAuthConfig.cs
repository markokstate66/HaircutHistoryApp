namespace HaircutHistoryApp.Services;

/// <summary>
/// Configuration for social authentication providers.
/// These values need to be set up in:
/// 1. Google Cloud Console (for Google Sign-In)
/// 2. Facebook Developer Console (for Facebook Login)
/// 3. Apple Developer Console (for Sign in with Apple)
/// 4. PlayFab Game Manager > Add-ons (to enable each provider)
/// </summary>
public static class SocialAuthConfig
{
    // Google OAuth 2.0 Client ID
    // Get from: https://console.cloud.google.com/apis/credentials
    // Create OAuth 2.0 Client ID for Android and iOS
    public const string GoogleClientId = "YOUR_GOOGLE_CLIENT_ID";

    // Facebook App ID
    // Get from: https://developers.facebook.com/apps/
    public const string FacebookAppId = "YOUR_FACEBOOK_APP_ID";

    // Apple Services ID (for Sign in with Apple)
    // Get from: https://developer.apple.com/account/resources/identifiers
    public const string AppleServicesId = "YOUR_APPLE_SERVICES_ID";

    // Redirect URI for OAuth callbacks
    // Must match what's configured in each provider's console
    public const string RedirectUri = "com.haircuthistory.app://callback";

    // PlayFab Title ID (already configured in PlayFabConfig)
    // Make sure to enable Google, Facebook, Apple in PlayFab Add-ons
}
