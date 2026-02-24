namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing advertisements
/// </summary>
public interface IAdService
{
    /// <summary>
    /// Whether the ad service has been initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Whether ads should be shown (false for premium users)
    /// </summary>
    bool ShouldShowAds { get; }

    /// <summary>
    /// Whether an interstitial ad is loaded and ready to show
    /// </summary>
    bool IsInterstitialLoaded { get; }

    /// <summary>
    /// Initialize the ad service
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Request to load a banner ad
    /// </summary>
    void LoadBannerAd();

    /// <summary>
    /// Hide the banner ad
    /// </summary>
    void HideBannerAd();

    /// <summary>
    /// Show the banner ad (if user is not premium)
    /// </summary>
    void ShowBannerAd();

    /// <summary>
    /// Load an interstitial ad for later display
    /// </summary>
    void LoadInterstitialAd();

    /// <summary>
    /// Show the interstitial ad if loaded and interval has passed
    /// </summary>
    /// <returns>True if the ad was shown</returns>
    Task<bool> TryShowInterstitialAdAsync();

    /// <summary>
    /// Force show the interstitial ad (ignores interval timer)
    /// </summary>
    /// <returns>True if the ad was shown</returns>
    Task<bool> ShowInterstitialAdAsync();

    /// <summary>
    /// Event fired when an interstitial ad is closed
    /// </summary>
    event EventHandler? InterstitialAdClosed;
}
