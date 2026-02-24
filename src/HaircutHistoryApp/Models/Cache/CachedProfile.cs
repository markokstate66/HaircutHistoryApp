using SQLite;

namespace HaircutHistoryApp.Models.Cache;

/// <summary>
/// SQLite entity for cached profile data.
/// </summary>
[Table("Profiles")]
public class CachedProfile
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string OwnerUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Measurements serialized as JSON.
    /// </summary>
    public string MeasurementsJson { get; set; } = "[]";

    public string? AvatarUrl { get; set; }
    public string? ImageUrl1 { get; set; }
    public string? ImageUrl2 { get; set; }
    public string? ImageUrl3 { get; set; }
    public int HaircutCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Hash of content for sync comparison.
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Current sync status of the entity.
    /// </summary>
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Synced;

    /// <summary>
    /// When this entity was last synced with server.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
