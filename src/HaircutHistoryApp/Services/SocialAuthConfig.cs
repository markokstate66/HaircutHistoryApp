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
    // Google OAuth 2.0 Client IDs
    // Get from: https://console.cloud.google.com/apis/credentials
#if ANDROID
    public const string GoogleClientId = "1002816079632-51s901e5jdcq7b97c2vkqm38p25ig20v.apps.googleusercontent.com";
#elif IOS
    public const string GoogleClientId = "1002816079632-ovdhkusbs1o0pevhq74andev4n9tsm62.apps.googleusercontent.com";
#else
    public const string GoogleClientId = "1002816079632-ppf6u4cuke8a5o0dfccp7pio70ronie6.apps.googleusercontent.com";
#endif

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
