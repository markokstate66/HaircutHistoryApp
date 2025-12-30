namespace HaircutHistoryApp.Models;

/// <summary>
/// Subscription tier levels for the application
/// </summary>
public enum SubscriptionTier
{
    /// <summary>
    /// Free tier: 5 profile limit, text-only, ads shown
    /// </summary>
    Free,

    /// <summary>
    /// Premium tier: Unlimited profiles, photo attachments, ad-free
    /// </summary>
    Premium
}

/// <summary>
/// State of a purchase transaction
/// </summary>
public enum PurchaseState
{
    Unknown,
    Purchased,
    Pending,
    Failed,
    Cancelled,
    Restored
}

/// <summary>
/// Contains subscription status and purchase details
/// </summary>
public class SubscriptionInfo
{
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
    public DateTime? ExpirationDate { get; set; }
    public string? TransactionId { get; set; }
    public string? ProductId { get; set; }
    public PurchaseState PurchaseState { get; set; } = PurchaseState.Unknown;
    public DateTime? PurchaseDate { get; set; }

    /// <summary>
    /// Returns true if the subscription has expired
    /// </summary>
    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    /// <summary>
    /// Returns true if the user has an active premium subscription
    /// </summary>
    public bool IsActive => Tier == SubscriptionTier.Premium && !IsExpired;
}

/// <summary>
/// Product information from the app store
/// </summary>
public class ProductInfo
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public decimal PriceValue { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}

/// <summary>
/// Result of a purchase attempt
/// </summary>
public class PurchaseResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public PurchaseState State { get; set; }
    public string? TransactionId { get; set; }
}

/// <summary>
/// Event args for subscription tier changes
/// </summary>
public class SubscriptionChangedEventArgs : EventArgs
{
    public SubscriptionTier OldTier { get; set; }
    public SubscriptionTier NewTier { get; set; }
}
