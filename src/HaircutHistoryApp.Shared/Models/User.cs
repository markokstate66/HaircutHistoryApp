using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.Models;

/// <summary>
/// Represents an authenticated user account from Azure AD B2C.
/// </summary>
public class User
{
    /// <summary>
    /// The B2C Object ID (unique identifier from Azure AD B2C).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user has an active premium subscription.
    /// </summary>
    [JsonPropertyName("isPremium")]
    public bool IsPremium { get; set; }

    /// <summary>
    /// When the premium subscription expires (null if not premium or lifetime).
    /// </summary>
    [JsonPropertyName("premiumExpiresAt")]
    public DateTime? PremiumExpiresAt { get; set; }

    /// <summary>
    /// When the user account was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the user account was last updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Checks if the user's premium subscription is currently active.
    /// </summary>
    [JsonIgnore]
    public bool IsPremiumActive => IsPremium && (PremiumExpiresAt == null || PremiumExpiresAt > DateTime.UtcNow);
}
