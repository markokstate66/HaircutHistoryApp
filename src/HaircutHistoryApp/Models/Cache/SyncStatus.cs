namespace HaircutHistoryApp.Models.Cache;

/// <summary>
/// Sync status for cached entities.
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// Entity is synchronized with server.
    /// </summary>
    Synced = 0,

    /// <summary>
    /// Entity has local changes pending upload.
    /// </summary>
    PendingUpload = 1,

    /// <summary>
    /// Entity is marked for deletion.
    /// </summary>
    PendingDelete = 2
}
