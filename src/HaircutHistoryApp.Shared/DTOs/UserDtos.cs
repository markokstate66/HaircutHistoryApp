using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.DTOs;

/// <summary>
/// Request to create or update the current user.
/// </summary>
public class CreateOrUpdateUserRequest
{
    /// <summary>
    /// Display name for the user.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}
