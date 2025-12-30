using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing user subscriptions and in-app purchases
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Current subscription tier of the user
    /// </summary>
    SubscriptionTier CurrentTier { get; }

    /// <summary>
    /// Returns true if the user has an active premium subscription
    /// </summary>
    bool IsPremium { get; }

    /// <summary>
    /// Current subscription details
    /// </summary>
    SubscriptionInfo? CurrentSubscription { get; }

    /// <summary>
    /// Fired when the subscription tier changes
    /// </summary>
    event EventHandler<SubscriptionChangedEventArgs>? SubscriptionChanged;

    /// <summary>
    /// Initialize the subscription service and sync with store
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Get the current subscription info from storage
    /// </summary>
    Task<SubscriptionInfo> GetSubscriptionInfoAsync();

    /// <summary>
    /// Check if the user can add a new profile based on their tier
    /// </summary>
    /// <param name="currentProfileCount">Current number of profiles the user has</param>
    Task<bool> CanAddProfileAsync(int currentProfileCount);

    /// <summary>
    /// Check if the user can add photos to profiles (premium feature)
    /// </summary>
    Task<bool> CanAddPhotosAsync();

    /// <summary>
    /// Get available subscription products from the app store
    /// </summary>
    Task<IEnumerable<ProductInfo>> GetAvailableProductsAsync();

    /// <summary>
    /// Purchase a premium subscription
    /// </summary>
    /// <param name="productId">The product ID to purchase</param>
    Task<PurchaseResult> PurchasePremiumAsync(string productId);

    /// <summary>
    /// Restore previous purchases (for reinstalls or new devices)
    /// </summary>
    Task<PurchaseResult> RestorePurchasesAsync();

    /// <summary>
    /// Validate subscription status with the store and sync
    /// </summary>
    Task<bool> ValidateAndSyncSubscriptionAsync();
}
