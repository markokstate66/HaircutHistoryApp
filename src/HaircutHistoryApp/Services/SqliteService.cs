using HaircutHistoryApp.Models.Cache;
using SQLite;

namespace HaircutHistoryApp.Services;

/// <summary>
/// SQLite database implementation for local caching.
/// </summary>
public class SqliteService : ISqliteService
{
    private SQLiteAsyncConnection? _database;
    private readonly ILogService _logService;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized;

    private const string DatabaseName = "haircuthistory.db3";
    private const string LastSyncTimeKey = "LastSyncTime";

    public SqliteService(ILogService logService)
    {
        _logService = logService;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseName);
            _logService.Info("SqliteService", $"Initializing database at: {databasePath}");

            _database = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            // Create tables
            await _database.CreateTableAsync<CachedProfile>();
            await _database.CreateTableAsync<CachedHaircutRecord>();
            await _database.CreateTableAsync<PendingOperation>();
            await _database.CreateTableAsync<SyncMetadata>();

            _isInitialized = true;
            _logService.Info("SqliteService", "Database initialized successfully");
        }
        catch (Exception ex)
        {
            _logService.Error("SqliteService", "Failed to initialize database", ex);
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_isInitialized)
            await InitializeAsync();
    }

    #region Profile Operations

    public async Task<List<CachedProfile>> GetProfilesAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<CachedProfile>().ToListAsync();
    }

    public async Task<CachedProfile?> GetProfileAsync(string id)
    {
        await EnsureInitializedAsync();
        return await _database!.Table<CachedProfile>().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task SaveProfileAsync(CachedProfile profile)
    {
        await EnsureInitializedAsync();
        var existing = await GetProfileAsync(profile.Id);
        if (existing != null)
        {
            await _database!.UpdateAsync(profile);
        }
        else
        {
            await _database!.InsertAsync(profile);
        }
    }

    public async Task DeleteProfileAsync(string id)
    {
        await EnsureInitializedAsync();
        await _database!.DeleteAsync<CachedProfile>(id);
        // Also delete associated records
        await DeleteRecordsForProfileAsync(id);
    }

    public async Task SaveProfilesBatchAsync(IEnumerable<CachedProfile> profiles)
    {
        await EnsureInitializedAsync();
        await _database!.RunInTransactionAsync(conn =>
        {
            foreach (var profile in profiles)
            {
                conn.InsertOrReplace(profile);
            }
        });
    }

    #endregion

    #region Haircut Record Operations

    public async Task<List<CachedHaircutRecord>> GetRecordsAsync(string profileId)
    {
        await EnsureInitializedAsync();
        return await _database!.Table<CachedHaircutRecord>()
            .Where(r => r.ProfileId == profileId)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    public async Task<CachedHaircutRecord?> GetRecordAsync(string id)
    {
        await EnsureInitializedAsync();
        return await _database!.Table<CachedHaircutRecord>().FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task SaveRecordAsync(CachedHaircutRecord record)
    {
        await EnsureInitializedAsync();
        var existing = await GetRecordAsync(record.Id);
        if (existing != null)
        {
            await _database!.UpdateAsync(record);
        }
        else
        {
            await _database!.InsertAsync(record);
        }
    }

    public async Task DeleteRecordAsync(string id)
    {
        await EnsureInitializedAsync();
        await _database!.DeleteAsync<CachedHaircutRecord>(id);
    }

    public async Task SaveRecordsBatchAsync(IEnumerable<CachedHaircutRecord> records)
    {
        await EnsureInitializedAsync();
        await _database!.RunInTransactionAsync(conn =>
        {
            foreach (var record in records)
            {
                conn.InsertOrReplace(record);
            }
        });
    }

    public async Task DeleteRecordsForProfileAsync(string profileId)
    {
        await EnsureInitializedAsync();
        await _database!.ExecuteAsync("DELETE FROM HaircutRecords WHERE ProfileId = ?", profileId);
    }

    #endregion

    #region Pending Operations

    public async Task<List<PendingOperation>> GetPendingOperationsAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<PendingOperation>()
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task AddPendingOperationAsync(PendingOperation op)
    {
        await EnsureInitializedAsync();
        op.CreatedAt = DateTime.UtcNow;
        await _database!.InsertAsync(op);
    }

    public async Task RemovePendingOperationAsync(int id)
    {
        await EnsureInitializedAsync();
        await _database!.DeleteAsync<PendingOperation>(id);
    }

    public async Task UpdatePendingOperationAsync(PendingOperation op)
    {
        await EnsureInitializedAsync();
        await _database!.UpdateAsync(op);
    }

    #endregion

    #region Sync Metadata

    public async Task<DateTime?> GetLastSyncTimeAsync()
    {
        var value = await GetSyncMetadataAsync(LastSyncTimeKey);
        if (value != null && DateTime.TryParse(value, out var time))
        {
            return time;
        }
        return null;
    }

    public async Task SetLastSyncTimeAsync(DateTime time)
    {
        await SetSyncMetadataAsync(LastSyncTimeKey, time.ToString("O"));
    }

    public async Task<string?> GetSyncMetadataAsync(string key)
    {
        await EnsureInitializedAsync();
        var metadata = await _database!.Table<SyncMetadata>().FirstOrDefaultAsync(m => m.Key == key);
        return metadata?.Value;
    }

    public async Task SetSyncMetadataAsync(string key, string value)
    {
        await EnsureInitializedAsync();
        var metadata = new SyncMetadata { Key = key, Value = value };
        var existing = await _database!.Table<SyncMetadata>().FirstOrDefaultAsync(m => m.Key == key);
        if (existing != null)
        {
            metadata.Id = existing.Id;
            await _database.UpdateAsync(metadata);
        }
        else
        {
            await _database.InsertAsync(metadata);
        }
    }

    #endregion

    #region Maintenance

    public async Task ClearAllDataAsync()
    {
        await EnsureInitializedAsync();
        _logService.Info("SqliteService", "Clearing all cached data");

        await _database!.DeleteAllAsync<CachedProfile>();
        await _database.DeleteAllAsync<CachedHaircutRecord>();
        await _database.DeleteAllAsync<PendingOperation>();
        await _database.DeleteAllAsync<SyncMetadata>();

        _logService.Info("SqliteService", "All cached data cleared");
    }

    public async Task<List<CachedProfile>> GetPendingProfilesAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<CachedProfile>()
            .Where(p => p.SyncStatus != SyncStatus.Synced)
            .ToListAsync();
    }

    public async Task<List<CachedHaircutRecord>> GetPendingRecordsAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<CachedHaircutRecord>()
            .Where(r => r.SyncStatus != SyncStatus.Synced)
            .ToListAsync();
    }

    #endregion
}

/// <summary>
/// SQLite table for sync metadata storage.
/// </summary>
[Table("SyncMetadata")]
public class SyncMetadata
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
