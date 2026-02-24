using SQLite;

namespace HaircutHistoryApp.Models.Cache;

/// <summary>
/// Represents a pending operation queued for background sync.
/// </summary>
[Table("PendingOperations")]
public class PendingOperation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Type of operation: Create, Update, Delete
    /// </summary>
    public OperationType OperationType { get; set; }

    /// <summary>
    /// Type of entity: Profile, HaircutRecord
    /// </summary>
    public EntityType EntityType { get; set; }

    /// <summary>
    /// ID of the entity being operated on.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// For HaircutRecords, the parent profile ID.
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// JSON payload of the entity data (for Create/Update).
    /// </summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// When this operation was queued.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of times this operation has been attempted.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Last error message if the operation failed.
    /// </summary>
    public string? LastError { get; set; }
}

/// <summary>
/// Type of pending operation.
/// </summary>
public enum OperationType
{
    Create = 0,
    Update = 1,
    Delete = 2
}

/// <summary>
/// Type of entity for pending operations.
/// </summary>
public enum EntityType
{
    Profile = 0,
    HaircutRecord = 1
}
