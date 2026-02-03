using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.Models;

/// <summary>
/// Represents a haircut template/recipe (e.g., "Dad's winter haircut", "Ryder's summer cut").
/// Contains the master measurements that define this haircut style.
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
    /// Name of the haircut profile (e.g., "Dad's winter haircut", "Ryder's summer cut").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description explaining why this cut or any special notes (e.g., "Shorter for baseball helmets").
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The master measurements/steps that define this haircut style.
    /// </summary>
    [JsonPropertyName("measurements")]
    public List<Measurement> Measurements { get; set; } = new();

    /// <summary>
    /// URL to the profile's avatar/photo image.
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
