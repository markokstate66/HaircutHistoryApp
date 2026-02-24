namespace HaircutHistoryApp.Services;

/// <summary>
/// Event args for sync completion.
/// </summary>
public class SyncCompletedEventArgs : EventArgs
{
    public bool Success { get; set; }
    public int ProfilesUpdated { get; set; }
    public int ProfilesDeleted { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Service for background sync between local cache and server.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Event fired when sync completes.
    /// </summary>
    event EventHandler<SyncCompletedEventArgs>? SyncCompleted;

    /// <summary>
    /// Whether a sync is currently in progress.
    /// </summary>
    bool IsSyncing { get; }

    /// <summary>
    /// Performs a background sync with the server.
    /// Downloads server changes and updates local cache.
    /// </summary>
    Task SyncAsync();

    /// <summary>
    /// Processes pending local operations (uploads).
    /// </summary>
    Task ProcessPendingOperationsAsync();

    /// <summary>
    /// Forces a full sync, ignoring cache.
    /// </summary>
    Task ForceSyncAsync();
}
