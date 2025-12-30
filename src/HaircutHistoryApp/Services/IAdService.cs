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
}
