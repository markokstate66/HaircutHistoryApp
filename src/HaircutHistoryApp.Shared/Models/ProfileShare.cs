using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.Models;

/// <summary>
/// Grants a stylist access to a specific profile.
/// </summary>
public class ProfileShare
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The ID of the profile being shared.
    /// </summary>
    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the stylist user who has access.
    /// </summary>
    [JsonPropertyName("stylistUserId")]
    public string StylistUserId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the share is currently active (can be revoked by client).
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the profile was shared.
    /// </summary>
    [JsonPropertyName("sharedAt")]
    public DateTime SharedAt { get; set; }

    /// <summary>
    /// When the share was revoked (null if still active).
    /// </summary>
    [JsonPropertyName("revokedAt")]
    public DateTime? RevokedAt { get; set; }
}

/// <summary>
/// Represents a share token for QR code generation.
/// </summary>
public class ShareToken
{
    /// <summary>
    /// The encrypted share token.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The deep link URL for sharing (haircuthistory://share/{token}).
    /// </summary>
    [JsonPropertyName("shareUrl")]
    public string ShareUrl { get; set; } = string.Empty;

    /// <summary>
    /// When the token expires.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}
