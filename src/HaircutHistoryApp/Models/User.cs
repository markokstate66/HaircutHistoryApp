namespace HaircutHistoryApp.Models;

public enum UserMode
{
    Client,
    Barber
}

public enum AuthProvider
{
    Email,      // PlayFab email/password
    Google,
    Facebook,
    Apple,
    Device      // Anonymous device login
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserMode Mode { get; set; } = UserMode.Client;
    public string? ShopName { get; set; }

    /// <summary>
    /// The authentication provider used to sign in
    /// </summary>
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Email;

    /// <summary>
    /// URL to the user's profile picture.
    /// For social logins (Google, Facebook, Apple), this is the provider's default picture.
    /// For Email/PlayFab logins, this can be a custom uploaded picture.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// URL to a custom uploaded profile picture (only used when AuthProvider is Email).
    /// Takes precedence over the provider's default picture if set.
    /// </summary>
    public string? CustomProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The user's subscription tier (Free or Premium)
    /// </summary>
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;

    /// <summary>
    /// When the premium subscription expires (null for free tier or lifetime)
    /// </summary>
    public DateTime? SubscriptionExpirationDate { get; set; }

    /// <summary>
    /// Transaction ID from the last successful subscription purchase
    /// </summary>
    public string? SubscriptionTransactionId { get; set; }

    /// <summary>
    /// Returns true if the user has an active premium subscription
    /// </summary>
    public bool IsPremium => SubscriptionTier == SubscriptionTier.Premium &&
        (!SubscriptionExpirationDate.HasValue || SubscriptionExpirationDate.Value > DateTime.UtcNow);

    /// <summary>
    /// Returns true if the user can add photos to profiles (premium feature)
    /// </summary>
    public bool CanAddPhotos => IsPremium;

    /// <summary>
    /// Gets the effective profile picture URL to display.
    /// Returns custom picture if available, otherwise the provider's default.
    /// </summary>
    public string? EffectiveProfilePictureUrl =>
        !string.IsNullOrEmpty(CustomProfilePictureUrl) ? CustomProfilePictureUrl : ProfilePictureUrl;

    /// <summary>
    /// Returns true if the user can upload a custom profile picture.
    /// Social login users use their provider's picture by default.
    /// </summary>
    public bool CanUploadCustomPicture => AuthProvider == AuthProvider.Email;
}
