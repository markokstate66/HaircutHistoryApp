using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.Models;

/// <summary>
/// Represents a person whose haircuts are being tracked (the user themselves, a child, etc.).
/// </summary>
public class Profile
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The ID of the user who owns this profile.
    /// </summary>
    [JsonPropertyName("ownerUserId")]
    public string OwnerUserId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the profile (e.g., "Me", "Marcus Jr", "Son").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL to the profile's avatar image.
    /// </summary>
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Whether this profile has been soft deleted.
    /// </summary>
    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the profile was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the profile was last updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Count of haircut records for this profile (populated by API).
    /// </summary>
    [JsonPropertyName("haircutCount")]
    public int HaircutCount { get; set; }
}
