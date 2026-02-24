using System.Text.Json.Serialization;
using HaircutHistoryApp.Shared.Models;

namespace HaircutHistoryApp.Shared.DTOs;

/// <summary>
/// Request to create a new profile (haircut template).
/// </summary>
public class CreateProfileRequest
{
    /// <summary>
    /// Name of the profile (e.g., "Dad's winter haircut", "Ryder's summer cut").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description explaining this haircut (e.g., "Shorter for baseball helmets").
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The master measurements/steps that define this haircut style.
    /// </summary>
    [JsonPropertyName("measurements")]
    public List<Measurement> Measurements { get; set; } = new();

    /// <summary>
    /// URL to the profile avatar (optional).
    /// </summary>
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Reference photo URL 1.
    /// </summary>
    [JsonPropertyName("imageUrl1")]
    public string? ImageUrl1 { get; set; }

    /// <summary>
    /// Reference photo URL 2.
    /// </summary>
    [JsonPropertyName("imageUrl2")]
    public string? ImageUrl2 { get; set; }

    /// <summary>
    /// Reference photo URL 3.
    /// </summary>
    [JsonPropertyName("imageUrl3")]
    public string? ImageUrl3 { get; set; }
}

/// <summary>
/// Request to update an existing profile.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// New name for the profile.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// New description for the profile.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Updated measurements for the profile.
    /// </summary>
    [JsonPropertyName("measurements")]
    public List<Measurement>? Measurements { get; set; }

    /// <summary>
    /// New avatar URL for the profile.
    /// </summary>
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Reference photo URL 1.
    /// </summary>
    [JsonPropertyName("imageUrl1")]
    public string? ImageUrl1 { get; set; }

    /// <summary>
    /// Reference photo URL 2.
    /// </summary>
    [JsonPropertyName("imageUrl2")]
    public string? ImageUrl2 { get; set; }

    /// <summary>
    /// Reference photo URL 3.
    /// </summary>
    [JsonPropertyName("imageUrl3")]
    public string? ImageUrl3 { get; set; }
}
