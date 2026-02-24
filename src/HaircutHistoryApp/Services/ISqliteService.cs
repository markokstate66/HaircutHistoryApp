using HaircutHistoryApp.Models.Cache;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for local SQLite database operations.
/// </summary>
public interface ISqliteService
{
    /// <summary>
    /// Initializes the database and creates tables.
    /// </summary>
    Task InitializeAsync();

    #region Profile Operations

    /// <summary>
    /// Gets all cached profiles.
    /// </summary>
    Task<List<CachedProfile>> GetProfilesAsync();

    /// <summary>
    /// Gets a specific cached profile by ID.
    /// </summary>
    Task<CachedProfile?> GetProfileAsync(string id);

    /// <summary>
    /// Saves a profile to the cache.
    /// </summary>
    Task SaveProfileAsync(CachedProfile profile);

    /// <summary>
    /// Deletes a profile from the cache.
    /// </summary>
    Task DeleteProfileAsync(string id);

    /// <summary>
    /// Saves multiple profiles in a batch.
    /// </summary>
    Task SaveProfilesBatchAsync(IEnumerable<CachedProfile> profiles);

    #endregion

    #region Haircut Record Operations

    /// <summary>
    /// Gets all cached haircut records for a profile.
    /// </summary>
    Task<List<CachedHaircutRecord>> GetRecordsAsync(string profileId);

    /// <summary>
    /// Gets a specific cached haircut record.
    /// </summary>
    Task<CachedHaircutRecord?> GetRecordAsync(string id);

    /// <summary>
    /// Saves a haircut record to the cache.
    /// </summary>
    Task SaveRecordAsync(CachedHaircutRecord record);

    /// <summary>
    /// Deletes a haircut record from the cache.
    /// </summary>
    Task DeleteRecordAsync(string id);

    /// <summary>
    /// Saves multiple haircut records in a batch.
    /// </summary>
    Task SaveRecordsBatchAsync(IEnumerable<CachedHaircutRecord> records);

    /// <summary>
    /// Deletes all records for a profile.
    /// </summary>
    Task DeleteRecordsForProfileAsync(string profileId);

    #endregion

    #region Pending Operations

    /// <summary>
    /// Gets all pending operations ordered by creation time.
    /// </summary>
    Task<List<PendingOperation>> GetPendingOperationsAsync();

    /// <summary>
    /// Adds a new pending operation to the queue.
    /// </summary>
    Task AddPendingOperationAsync(PendingOperation op);

    /// <summary>
    /// Removes a pending operation after successful sync.
    /// </summary>
    Task RemovePendingOperationAsync(int id);

    /// <summary>
    /// Updates retry count and error for a failed operation.
    /// </summary>
    Task UpdatePendingOperationAsync(PendingOperation op);

    #endregion

    #region Sync Metadata

    /// <summary>
    /// Gets the last sync time.
    /// </summary>
    Task<DateTime?> GetLastSyncTimeAsync();

    /// <summary>
    /// Sets the last sync time.
    /// </summary>
    Task SetLastSyncTimeAsync(DateTime time);

    /// <summary>
    /// Gets a sync metadata value.
    /// </summary>
    Task<string?> GetSyncMetadataAsync(string key);

    /// <summary>
    /// Sets a sync metadata value.
    /// </summary>
    Task SetSyncMetadataAsync(string key, string value);

    #endregion

    #region Maintenance

    /// <summary>
    /// Clears all cached data (for logout).
    /// </summary>
    Task ClearAllDataAsync();

    /// <summary>
    /// Gets profiles with pending changes.
    /// </summary>
    Task<List<CachedProfile>> GetPendingProfilesAsync();

    /// <summary>
    /// Gets records with pending changes.
    /// </summary>
    Task<List<CachedHaircutRecord>> GetPendingRecordsAsync();

    #endregion
}
