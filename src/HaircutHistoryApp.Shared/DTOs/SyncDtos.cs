using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.DTOs;

/// <summary>
/// Lightweight profile info for sync comparison.
/// </summary>
public class ProfileSyncInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("contentHash")]
    public string ContentHash { get; set; } = string.Empty;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Response from sync endpoint.
/// </summary>
public class SyncResponse
{
    [JsonPropertyName("profiles")]
    public List<ProfileSyncInfo> Profiles { get; set; } = new();

    [JsonPropertyName("serverTime")]
    public DateTime ServerTime { get; set; }
}

/// <summary>
/// Request to fetch multiple profiles by ID.
/// </summary>
public class BatchFetchRequest
{
    [JsonPropertyName("ids")]
    public List<string> Ids { get; set; } = new();
}

/// <summary>
/// Lightweight haircut record info for sync comparison.
/// </summary>
public class HaircutRecordSyncInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; } = string.Empty;

    [JsonPropertyName("contentHash")]
    public string ContentHash { get; set; } = string.Empty;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Request to fetch multiple haircut records by ID.
/// </summary>
public class BatchRecordFetchRequest
{
    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; } = string.Empty;

    [JsonPropertyName("ids")]
    public List<string> Ids { get; set; } = new();
}
