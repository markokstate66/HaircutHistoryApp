namespace HaircutHistoryApp.Services;

/// <summary>
/// Configuration constants for subscription and advertising
/// </summary>
public static class SubscriptionConfig
{
    // ============================================
    // IN-APP PURCHASE PRODUCT IDS
    // Must match App Store Connect / Google Play Console
    // ============================================

    /// <summary>
    /// Monthly premium subscription product ID
    /// </summary>
    public const string PremiumMonthlyProductId = "com.haircuthistory.premium.monthly";

    /// <summary>
    /// Yearly premium subscription product ID
    /// </summary>
    public const string PremiumYearlyProductId = "com.haircuthistory.premium.yearly";

    /// <summary>
    /// All available subscription product IDs
    /// </summary>
    public static readonly string[] AllProductIds =
    {
        PremiumMonthlyProductId,
        PremiumYearlyProductId
    };

    // ============================================
    // PROFILE LIMITS
    // ============================================

    /// <summary>
    /// Maximum number of haircut profiles for free tier users
    /// </summary>
    public const int FreeProfileLimit = 1;

    /// <summary>
    /// Maximum number of haircut profiles for premium tier users
    /// </summary>
    public const int PremiumProfileLimit = 5;

    // ============================================
    // PLAYFAB DATA KEYS
    // ============================================

    /// <summary>
    /// PlayFab player data key for subscription info
    /// </summary>
    public const string SubscriptionDataKey = "subscription_info";

    // ============================================
    // ADMOB CONFIGURATION
    // Replace with real IDs before release
    // ============================================

    /// <summary>
    /// AdMob Application ID for Android
    /// </summary>
    public const string AdMobAppIdAndroid = "ca-app-pub-XXXXXXXXXXXXXXXX~YYYYYYYYYY";

    /// <summary>
    /// AdMob Application ID for iOS
    /// </summary>
    public const string AdMobAppIdIos = "ca-app-pub-XXXXXXXXXXXXXXXX~ZZZZZZZZZZ";

    /// <summary>
    /// Banner ad unit ID for Android (production)
    /// </summary>
    public const string BannerAdUnitIdAndroid = "ca-app-pub-XXXXXXXXXXXXXXXX/AAAAAAAAAA";

    /// <summary>
    /// Banner ad unit ID for iOS (production)
    /// </summary>
    public const string BannerAdUnitIdIos = "ca-app-pub-XXXXXXXXXXXXXXXX/BBBBBBBBBB";

    // ============================================
    // TEST AD UNIT IDS (use during development)
    // These are official Google test IDs
    // ============================================

    /// <summary>
    /// Test banner ad unit ID for Android
    /// </summary>
    public const string TestBannerAdUnitIdAndroid = "ca-app-pub-3940256099942544/6300978111";

    /// <summary>
    /// Test banner ad unit ID for iOS
    /// </summary>
    public const string TestBannerAdUnitIdIos = "ca-app-pub-3940256099942544/2934735716";

    // ============================================
    // HELPER METHODS
    // ============================================

    /// <summary>
    /// Gets the appropriate banner ad unit ID for the current platform
    /// </summary>
    /// <param name="useTestAds">Whether to use test ad units</param>
    public static string GetBannerAdUnitId(bool useTestAds = false)
    {
#if ANDROID
        return useTestAds ? TestBannerAdUnitIdAndroid : BannerAdUnitIdAndroid;
#elif IOS
        return useTestAds ? TestBannerAdUnitIdIos : BannerAdUnitIdIos;
#else
        return TestBannerAdUnitIdAndroid;
#endif
    }

    /// <summary>
    /// Gets the appropriate AdMob app ID for the current platform
    /// </summary>
    public static string GetAdMobAppId()
    {
#if ANDROID
        return AdMobAppIdAndroid;
#elif IOS
        return AdMobAppIdIos;
#else
        return AdMobAppIdAndroid;
#endif
    }

    /// <summary>
    /// Checks if a product ID is for a yearly subscription
    /// </summary>
    public static bool IsYearlyProduct(string productId)
    {
        return productId == PremiumYearlyProductId;
    }

    /// <summary>
    /// Gets the subscription duration in days based on product ID
    /// </summary>
    public static int GetSubscriptionDurationDays(string productId)
    {
        return IsYearlyProduct(productId) ? 365 : 30;
    }
}
