using System.Text.Json;
using System.Text.Json.Serialization;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Models.Cache;
using HaircutHistoryApp.Shared.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Data service implementation with local SQLite caching.
/// Provides instant UI updates while syncing in background.
/// </summary>
public class CachedDataService : IDataService
{
    private readonly ISqliteService _sqlite;
    private readonly AzureDataService _remote;
    private readonly ISyncService _sync;
    private readonly ILogService _logService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string? LastError { get; private set; }

    public CachedDataService(
        ISqliteService sqlite,
        AzureDataService remote,
        ISyncService sync,
        ILogService logService)
    {
        _sqlite = sqlite;
        _remote = remote;
        _sync = sync;
        _logService = logService;

        // Initialize database on construction
        _ = _sqlite.InitializeAsync();
    }

    #region Profile Operations

    public async Task<List<Models.Profile>> GetProfilesAsync()
    {
        LastError = null;
        try
        {
            // Ensure database is initialized
            await _sqlite.InitializeAsync();

            // Return from cache immediately
            var cached = await _sqlite.GetProfilesAsync();
            var profiles = cached
                .Where(p => p.SyncStatus != SyncStatus.PendingDelete)
                .Select(MapToProfile)
                .ToList();

            _logService.Info("CachedDataService", $"Returning {profiles.Count} profiles from cache");

            // Trigger background sync (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _sync.SyncAsync();
                }
                catch (Exception ex)
                {
                    _logService.Error("CachedDataService", "Background sync failed", ex);
                }
            });

            return profiles;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("CachedDataService", "GetProfilesAsync failed, falling back to remote", ex);

            // Fallback to remote on cache failure
            return await _remote.GetProfilesAsync();
        }
    }

    public async Task<Models.Profile?> GetProfileAsync(string profileId)
    {
        LastError = null;
        try
        {
            await _sqlite.InitializeAsync();

            var cached = await _sqlite.GetProfileAsync(profileId);
            if (cached != null && cached.SyncStatus != SyncStatus.PendingDelete)
            {
                return MapToProfile(cached);
            }

            // Not in cache, try remote
            var profile = await _remote.GetProfileAsync(profileId);
            if (profile != null)
            {
                // Cache it
                var toCache = MapToCachedProfile(profile);
                toCache.SyncStatus = SyncStatus.Synced;
                toCache.LastSyncedAt = DateTime.UtcNow;
                await _sqlite.SaveProfileAsync(toCache);
            }

            return profile;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("CachedDataService", "GetProfileAsync failed", ex);
            return await _remote.GetProfileAsync(profileId);
        }
    }

    public async Task<bool> SaveProfileAsync(Models.Profile profile)
    {
        LastError = null;
        try
        {
            await _sqlite.InitializeAsync();

            var isNew = string.IsNullOrEmpty(profile.Id);
            if (isNew)
            {
                profile.Id = Guid.NewGuid().ToString();
                profile.CreatedAt = DateTime.UtcNow;
            }
            profile.UpdatedAt = DateTime.UtcNow;

            // Save to cache immediately
            var cached = MapToCachedProfile(profile);
            cached.SyncStatus = SyncStatus.PendingUpload;
            cached.ContentHash = SyncService.ComputeContentHash(profile);
            await _sqlite.SaveProfileAsync(cached);

            _logService.Info("CachedDataService", $"Saved profile {profile.Id} to cache with status PendingUpload");

            // Queue for background upload
            var operation = new PendingOperation
            {
                OperationType = isNew ? OperationType.Create : OperationType.Update,
                EntityType = EntityType.Profile,
                EntityId = profile.Id,
                PayloadJson = JsonSerializer.Serialize(profile, JsonOptions)
            };
            await _sqlite.AddPendingOperationAsync(operation);

            // Process queue in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _sync.ProcessPendingOperationsAsync();
                }
                catch (Exception ex)
                {
                    _logService.Error("CachedDataService", "Background upload failed", ex);
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("CachedDataService", "SaveProfileAsync failed, trying remote", ex);

            // Fallback to direct remote save
            return await _remote.SaveProfileAsync(profile);
        }
    }

    public async Task<bool> DeleteProfileAsync(string profileId)
    {
        LastError = null;
        try
        {
            await _sqlite.InitializeAsync();

            // Mark as pending delete in cache
            var cached = await _sqlite.GetProfileAsync(profileId);
            if (cached != null)
            {
                cached.SyncStatus = SyncStatus.PendingDelete;
                await _sqlite.SaveProfileAsync(cached);
            }

            _logService.Info("CachedDataService", $"Marked profile {profileId} for deletion");

            // Queue delete operation
            var operation = new PendingOperation
            {
                OperationType = OperationType.Delete,
                EntityType = EntityType.Profile,
                EntityId = profileId
            };
            await _sqlite.AddPendingOperationAsync(operation);

            // Process in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _sync.ProcessPendingOperationsAsync();
                }
                catch (Exception ex)
                {
                    _logService.Error("CachedDataService", "Background delete failed", ex);
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("CachedDataService", "DeleteProfileAsync failed, trying remote", ex);
            return await _remote.DeleteProfileAsync(profileId);
        }
    }

    #endregion

    #region Haircut Record Operations

    public async Task<List<Models.HaircutRecord>> GetHaircutRecordsAsync(string profileId)
    {
        LastError = null;
        try
        {
            await _sqlite.InitializeAsync();

            // Return from cache immediately
            var cached = await _sqlite.GetRecordsAsync(profileId);
            var records = cached
                .Where(r => r.SyncStatus != SyncStatus.PendingDelete)
                .Select(MapToHaircutRecord)
                .ToList();

            _logService.Info("CachedDataService", $"Returning {records.Count} records from cache for profile {profileId}");

            // If cache is empty, fetch from remote
            if (records.Count == 0)
            {
                var remoteRecords = await _remote.GetHaircutRecordsAsync(profileId);
                if (remoteRecords.Count > 0)
                {
                    // Cache them
                    var toCache = remoteRecords.Select(r => MapToCachedRecord(r, SyncStatus.Synced)).ToList();
                    await _sqlite.SaveRecordsBatchAsync(toCache);
                    return remoteRecords;
                }
            }

            return records;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("CachedDataService", "GetHaircutRecordsAsync failed", ex);
            return await _remote.GetHaircutRecordsAsync(profileId);
        }
    }

    public async Task<Models.HaircutRecord?> GetHaircutRecordAsync(string profileId, string recordId)
    {
        LastError = null;
        try
        {
            await _sqlite.InitializeAsync();

            var cached = await _sqlite.GetRecordAsync(recordId);
            if (cached != null && cached.SyncStatus != SyncStatus.PendingDelete)
            {
                return MapToHaircutRecord(cached);
            }

            // Not in cache, try remote
            var record = await _remote.GetHaircutRecordAsync(profileId, recordId);
            if (record != null)
            {
                var toCache = MapToCachedRecord(record, SyncStatus.Synced);
                await _sqlite.SaveRecordAsync(toCache);
            }

            return record;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("CachedDataService", "GetHaircutRecordAsync failed", ex);
            return await _remote.GetHaircutRecordAsync(profileId, recordId);
        }
    }

    public async Task<bool> SaveHaircutRecordAsync(string profileId, Models.HaircutRecord record)
    {
        LastError = null;
        try
        {
            await _sqlite.InitializeAsync();

            var isNew = string.IsNullOrEmpty(record.Id);
            if (isNew)
            {
                record.Id = Guid.NewGuid().ToString();
                record.ProfileId = profileId;
                record.CreatedAt = DateTime.UtcNow;
            }
            record.UpdatedAt = DateTime.UtcNow;

            // Save to cache immediately
            var cached = MapToCachedRecord(record, SyncStatus.PendingUpload);
            cached.ContentHash = SyncService.ComputeContentHash(record);
            await _sqlite.SaveRecordAsync(cached);

            _logService.Info("CachedDataService", $"Saved record {record.Id} to cache with status PendingUpload");

            // Queue for background upload
            var operation = new PendingOperation
            {
                OperationType = isNew ? OperationType.Create : OperationType.Update,
                EntityType = EntityType.HaircutRecord,
                EntityId = record.Id,
                ParentId = profileId,
                PayloadJson = JsonSerializer.Serialize(record, JsonOptions)
            };
            await _sqlite.AddPendingOperationAsync(operation);

            // Process queue in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _sync.ProcessPendingOperationsAsync();
                }
                catch (Exception ex)
                {
                    _logService.Error("CachedDataService", "Background upload failed", ex);
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("CachedDataService", "SaveHaircutRecordAsync failed", ex);
            return await _remote.SaveHaircutRecordAsync(profileId, record);
        }
    }

    public async Task<bool> DeleteHaircutRecordAsync(string profileId, string recordId)
    {
        LastError = null;
        try
        {
            await _sqlite.InitializeAsync();

            // Mark as pending delete
            var cached = await _sqlite.GetRecordAsync(recordId);
            if (cached != null)
            {
                cached.SyncStatus = SyncStatus.PendingDelete;
                await _sqlite.SaveRecordAsync(cached);
            }

            _logService.Info("CachedDataService", $"Marked record {recordId} for deletion");

            // Queue delete operation
            var operation = new PendingOperation
            {
                OperationType = OperationType.Delete,
                EntityType = EntityType.HaircutRecord,
                EntityId = recordId,
                ParentId = profileId
            };
            await _sqlite.AddPendingOperationAsync(operation);

            // Process in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _sync.ProcessPendingOperationsAsync();
                }
                catch (Exception ex)
                {
                    _logService.Error("CachedDataService", "Background delete failed", ex);
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("CachedDataService", "DeleteHaircutRecordAsync failed", ex);
            return await _remote.DeleteHaircutRecordAsync(profileId, recordId);
        }
    }

    #endregion

    #region Share Operations

    public Task<ShareSession> CreateShareSessionAsync(string profileId)
    {
        // Share operations go directly to remote
        return _remote.CreateShareSessionAsync(profileId);
    }

    public Task<bool> AcceptShareAsync(string token)
    {
        return _remote.AcceptShareAsync(token);
    }

    public Task<List<Models.Profile>> GetSharedProfilesAsync()
    {
        // Shared profiles not cached locally
        return _remote.GetSharedProfilesAsync();
    }

    #endregion

    #region Mapping Helpers

    private static Models.Profile MapToProfile(CachedProfile cached)
    {
        var measurements = string.IsNullOrEmpty(cached.MeasurementsJson)
            ? new List<HaircutMeasurement>()
            : JsonSerializer.Deserialize<List<HaircutMeasurement>>(cached.MeasurementsJson, JsonOptions) ?? new List<HaircutMeasurement>();

        return new Models.Profile
        {
            Id = cached.Id,
            OwnerUserId = cached.OwnerUserId,
            Name = cached.Name,
            Description = cached.Description,
            Measurements = measurements,
            AvatarUrl = cached.AvatarUrl,
            ImageUrl1 = cached.ImageUrl1,
            ImageUrl2 = cached.ImageUrl2,
            ImageUrl3 = cached.ImageUrl3,
            HaircutCount = cached.HaircutCount,
            CreatedAt = cached.CreatedAt,
            UpdatedAt = cached.UpdatedAt
        };
    }

    private static CachedProfile MapToCachedProfile(Models.Profile profile)
    {
        return new CachedProfile
        {
            Id = profile.Id,
            OwnerUserId = profile.OwnerUserId,
            Name = profile.Name,
            Description = profile.Description,
            MeasurementsJson = JsonSerializer.Serialize(profile.Measurements, JsonOptions),
            AvatarUrl = profile.AvatarUrl,
            ImageUrl1 = profile.ImageUrl1,
            ImageUrl2 = profile.ImageUrl2,
            ImageUrl3 = profile.ImageUrl3,
            HaircutCount = profile.HaircutCount,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    private static Models.HaircutRecord MapToHaircutRecord(CachedHaircutRecord cached)
    {
        var photoUrls = string.IsNullOrEmpty(cached.PhotoUrlsJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(cached.PhotoUrlsJson, JsonOptions) ?? new List<string>();

        return new Models.HaircutRecord
        {
            Id = cached.Id,
            ProfileId = cached.ProfileId,
            CreatedByUserId = cached.CreatedByUserId,
            Date = cached.Date,
            StylistName = cached.StylistName,
            Location = cached.Location,
            PhotoUrls = photoUrls,
            Notes = cached.Notes,
            Price = cached.Price,
            DurationMinutes = cached.DurationMinutes,
            CreatedAt = cached.CreatedAt,
            UpdatedAt = cached.UpdatedAt
        };
    }

    private static CachedHaircutRecord MapToCachedRecord(Models.HaircutRecord record, SyncStatus status)
    {
        return new CachedHaircutRecord
        {
            Id = record.Id,
            ProfileId = record.ProfileId,
            CreatedByUserId = record.CreatedByUserId,
            Date = record.Date,
            StylistName = record.StylistName,
            Location = record.Location,
            PhotoUrlsJson = JsonSerializer.Serialize(record.PhotoUrls, JsonOptions),
            Notes = record.Notes,
            Price = record.Price,
            DurationMinutes = record.DurationMinutes,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            SyncStatus = status,
            LastSyncedAt = status == SyncStatus.Synced ? DateTime.UtcNow : null
        };
    }

    #endregion
}
