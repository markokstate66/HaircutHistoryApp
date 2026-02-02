using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.DTOs;

/// <summary>
/// Request to create a new profile.
/// </summary>
public class CreateProfileRequest
{
    /// <summary>
    /// Name of the profile (e.g., "Me", "Marcus Jr").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL to the profile avatar (optional).
    /// </summary>
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }
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
    /// New avatar URL for the profile.
    /// </summary>
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }
}
