namespace HaircutHistoryApp.Services;

/// <summary>
/// Configuration constants for subscription and advertising
/// </summary>
public static class SubscriptionConfig
{
    // ============================================
    // IN-APP PURCHASE PRODUCT IDS
    // Must match App Store Connect / Google Play Console
    // Pricing: Monthly $0.99, Yearly $4.99, Lifetime $9.99
    // ============================================

    /// <summary>
    /// Monthly premium subscription product ID ($0.99/month)
    /// </summary>
    public const string PremiumMonthlyProductId = "com.stg.haircuthistory.premium.monthly";

    /// <summary>
    /// Yearly premium subscription product ID ($4.99/year)
    /// </summary>
    public const string PremiumYearlyProductId = "com.stg.haircuthistory.premium.yearly";

    /// <summary>
    /// Lifetime premium purchase product ID ($9.99 one-time)
    /// </summary>
    public const string PremiumLifetimeProductId = "com.stg.haircuthistory.premium.lifetime";

    /// <summary>
    /// All available subscription product IDs
    /// </summary>
    public static readonly string[] AllProductIds =
    {
        PremiumMonthlyProductId,
        PremiumYearlyProductId,
        PremiumLifetimeProductId
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
    public const string AdMobAppIdAndroid = "ca-app-pub-6676281664229738~8089129883";

    /// <summary>
    /// AdMob Application ID for iOS
    /// </summary>
    public const string AdMobAppIdIos = "ca-app-pub-6676281664229738~9052230606";

    /// <summary>
    /// Banner ad unit ID for Android (production)
    /// </summary>
    public const string BannerAdUnitIdAndroid = "ca-app-pub-6676281664229738/6237391547";

    /// <summary>
    /// Banner ad unit ID for iOS (production)
    /// </summary>
    public const string BannerAdUnitIdIos = "ca-app-pub-6676281664229738/6332770056";

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

    /// <summary>
    /// Interstitial ad unit ID for Android (production)
    /// </summary>
    public const string InterstitialAdUnitIdAndroid = "ca-app-pub-6676281664229738/1523721539";

    /// <summary>
    /// Interstitial ad unit ID for iOS (production)
    /// </summary>
    public const string InterstitialAdUnitIdIos = "ca-app-pub-6676281664229738/3738649588";

    /// <summary>
    /// Test interstitial ad unit ID for Android
    /// </summary>
    public const string TestInterstitialAdUnitIdAndroid = "ca-app-pub-3940256099942544/1033173712";

    /// <summary>
    /// Test interstitial ad unit ID for iOS
    /// </summary>
    public const string TestInterstitialAdUnitIdIos = "ca-app-pub-3940256099942544/4411468910";

    /// <summary>
    /// Minimum interval between interstitial ads (in seconds)
    /// </summary>
    public const int InterstitialAdIntervalSeconds = 600; // 10 minutes

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
    /// Gets the appropriate interstitial ad unit ID for the current platform
    /// </summary>
    /// <param name="useTestAds">Whether to use test ad units</param>
    public static string GetInterstitialAdUnitId(bool useTestAds = false)
    {
#if ANDROID
        return useTestAds ? TestInterstitialAdUnitIdAndroid : InterstitialAdUnitIdAndroid;
#elif IOS
        return useTestAds ? TestInterstitialAdUnitIdIos : InterstitialAdUnitIdIos;
#else
        return TestInterstitialAdUnitIdAndroid;
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
    /// Checks if a product ID is for a lifetime purchase
    /// </summary>
    public static bool IsLifetimeProduct(string productId)
    {
        return productId == PremiumLifetimeProductId;
    }

    /// <summary>
    /// Gets the subscription duration in days based on product ID
    /// </summary>
    public static int GetSubscriptionDurationDays(string productId)
    {
        if (IsLifetimeProduct(productId))
            return 36500; // ~100 years
        return IsYearlyProduct(productId) ? 365 : 30;
    }
}
