using SQLite;

namespace HaircutHistoryApp.Models.Cache;

/// <summary>
/// SQLite entity for cached haircut record data.
/// </summary>
[Table("HaircutRecords")]
public class CachedHaircutRecord
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string ProfileId { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? StylistName { get; set; }
    public string? Location { get; set; }

    /// <summary>
    /// Photo URLs serialized as JSON array.
    /// </summary>
    public string PhotoUrlsJson { get; set; } = "[]";

    public string? Notes { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
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
