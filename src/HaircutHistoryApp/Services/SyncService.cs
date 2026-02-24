using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Models.Cache;
using HaircutHistoryApp.Shared.DTOs;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for background sync between local cache and server.
/// </summary>
public class SyncService : ISyncService
{
    private readonly ISqliteService _sqlite;
    private readonly IApiService _apiService;
    private readonly ILogService _logService;
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private bool _isSyncing;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public event EventHandler<SyncCompletedEventArgs>? SyncCompleted;

    public bool IsSyncing => _isSyncing;

    public SyncService(ISqliteService sqlite, IApiService apiService, ILogService logService)
    {
        _sqlite = sqlite;
        _apiService = apiService;
        _logService = logService;
    }

    public async Task SyncAsync()
    {
        if (!await _syncLock.WaitAsync(0))
        {
            _logService.Info("SyncService", "Sync already in progress, skipping");
            return;
        }

        try
        {
            _isSyncing = true;
            _logService.Info("SyncService", "Starting sync");

            // First, process any pending uploads
            await ProcessPendingOperationsInternalAsync();

            // Then, download server changes
            var result = await DownloadServerChangesAsync();

            _logService.Info("SyncService", $"Sync completed: {result.ProfilesUpdated} updated, {result.ProfilesDeleted} deleted");

            SyncCompleted?.Invoke(this, result);
        }
        catch (Exception ex)
        {
            _logService.Error("SyncService", "Sync failed", ex);
            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
        finally
        {
            _isSyncing = false;
            _syncLock.Release();
        }
    }

    public async Task ProcessPendingOperationsAsync()
    {
        if (!await _syncLock.WaitAsync(0))
        {
            _logService.Info("SyncService", "Sync in progress, queuing pending operations");
            return;
        }

        try
        {
            _isSyncing = true;
            await ProcessPendingOperationsInternalAsync();
        }
        finally
        {
            _isSyncing = false;
            _syncLock.Release();
        }
    }

    public async Task ForceSyncAsync()
    {
        await _syncLock.WaitAsync();
        try
        {
            _isSyncing = true;
            _logService.Info("SyncService", "Starting force sync");

            // Clear last sync time to force full download
            await _sqlite.SetSyncMetadataAsync("LastSyncTime", "");

            await ProcessPendingOperationsInternalAsync();
            var result = await DownloadServerChangesAsync();

            _logService.Info("SyncService", $"Force sync completed: {result.ProfilesUpdated} updated, {result.ProfilesDeleted} deleted");

            SyncCompleted?.Invoke(this, result);
        }
        catch (Exception ex)
        {
            _logService.Error("SyncService", "Force sync failed", ex);
            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
        finally
        {
            _isSyncing = false;
            _syncLock.Release();
        }
    }

    private async Task ProcessPendingOperationsInternalAsync()
    {
        var operations = await _sqlite.GetPendingOperationsAsync();
        if (operations.Count == 0)
        {
            _logService.Info("SyncService", "No pending operations");
            return;
        }

        _logService.Info("SyncService", $"Processing {operations.Count} pending operations");

        foreach (var op in operations)
        {
            try
            {
                var success = await ProcessOperationAsync(op);
                if (success)
                {
                    await _sqlite.RemovePendingOperationAsync(op.Id);
                    _logService.Info("SyncService", $"Completed operation {op.Id}: {op.OperationType} {op.EntityType}");
                }
                else
                {
                    op.RetryCount++;
                    await _sqlite.UpdatePendingOperationAsync(op);
                    _logService.Warning("SyncService", $"Operation {op.Id} failed, retry count: {op.RetryCount}");
                }
            }
            catch (Exception ex)
            {
                op.RetryCount++;
                op.LastError = ex.Message;
                await _sqlite.UpdatePendingOperationAsync(op);
                _logService.Error("SyncService", $"Operation {op.Id} threw exception", ex);
            }
        }
    }

    private async Task<bool> ProcessOperationAsync(PendingOperation op)
    {
        switch (op.EntityType)
        {
            case EntityType.Profile:
                return await ProcessProfileOperationAsync(op);
            case EntityType.HaircutRecord:
                return await ProcessHaircutRecordOperationAsync(op);
            default:
                _logService.Warning("SyncService", $"Unknown entity type: {op.EntityType}");
                return false;
        }
    }

    private async Task<bool> ProcessProfileOperationAsync(PendingOperation op)
    {
        switch (op.OperationType)
        {
            case OperationType.Create:
            case OperationType.Update:
                if (string.IsNullOrEmpty(op.PayloadJson))
                    return false;

                var profile = JsonSerializer.Deserialize<Profile>(op.PayloadJson, JsonOptions);
                if (profile == null)
                    return false;

                var request = new CreateProfileRequest
                {
                    Name = profile.Name,
                    Description = profile.Description,
                    Measurements = profile.Measurements.Select(m => new Shared.Models.Measurement
                    {
                        Area = m.Area,
                        GuardSize = m.GuardSize,
                        Technique = m.Technique,
                        Notes = m.Notes,
                        StepOrder = m.StepOrder
                    }).ToList(),
                    AvatarUrl = profile.AvatarUrl,
                    ImageUrl1 = profile.ImageUrl1,
                    ImageUrl2 = profile.ImageUrl2,
                    ImageUrl3 = profile.ImageUrl3
                };

                if (op.OperationType == OperationType.Create)
                {
                    var createResponse = await _apiService.CreateProfileAsync(request);
                    if (createResponse.Success && createResponse.Data != null)
                    {
                        // Update local cache with server-assigned ID
                        var cached = await _sqlite.GetProfileAsync(op.EntityId);
                        if (cached != null)
                        {
                            await _sqlite.DeleteProfileAsync(op.EntityId);
                            cached.Id = createResponse.Data.Id;
                            cached.SyncStatus = SyncStatus.Synced;
                            cached.ContentHash = createResponse.Data.ContentHash;
                            cached.LastSyncedAt = DateTime.UtcNow;
                            await _sqlite.SaveProfileAsync(cached);
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    var updateRequest = new UpdateProfileRequest
                    {
                        Name = request.Name,
                        Description = request.Description,
                        Measurements = request.Measurements,
                        AvatarUrl = request.AvatarUrl,
                        ImageUrl1 = request.ImageUrl1,
                        ImageUrl2 = request.ImageUrl2,
                        ImageUrl3 = request.ImageUrl3
                    };
                    var updateResponse = await _apiService.UpdateProfileAsync(op.EntityId, updateRequest);
                    if (updateResponse.Success)
                    {
                        var cached = await _sqlite.GetProfileAsync(op.EntityId);
                        if (cached != null)
                        {
                            cached.SyncStatus = SyncStatus.Synced;
                            cached.ContentHash = updateResponse.Data?.ContentHash ?? cached.ContentHash;
                            cached.LastSyncedAt = DateTime.UtcNow;
                            await _sqlite.SaveProfileAsync(cached);
                        }
                        return true;
                    }
                    return false;
                }

            case OperationType.Delete:
                var deleteResponse = await _apiService.DeleteProfileAsync(op.EntityId);
                if (deleteResponse.Success)
                {
                    await _sqlite.DeleteProfileAsync(op.EntityId);
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    private async Task<bool> ProcessHaircutRecordOperationAsync(PendingOperation op)
    {
        if (string.IsNullOrEmpty(op.ParentId))
        {
            _logService.Warning("SyncService", "HaircutRecord operation missing ParentId");
            return false;
        }

        switch (op.OperationType)
        {
            case OperationType.Create:
            case OperationType.Update:
                if (string.IsNullOrEmpty(op.PayloadJson))
                    return false;

                var record = JsonSerializer.Deserialize<HaircutRecord>(op.PayloadJson, JsonOptions);
                if (record == null)
                    return false;

                var request = new CreateHaircutRecordRequest
                {
                    Date = record.Date,
                    StylistName = record.StylistName,
                    Location = record.Location,
                    Notes = record.Notes,
                    Price = record.Price,
                    DurationMinutes = record.DurationMinutes,
                    PhotoUrls = record.PhotoUrls
                };

                if (op.OperationType == OperationType.Create)
                {
                    var createResponse = await _apiService.CreateHaircutRecordAsync(op.ParentId, request);
                    if (createResponse.Success && createResponse.Data != null)
                    {
                        var cached = await _sqlite.GetRecordAsync(op.EntityId);
                        if (cached != null)
                        {
                            await _sqlite.DeleteRecordAsync(op.EntityId);
                            cached.Id = createResponse.Data.Id;
                            cached.SyncStatus = SyncStatus.Synced;
                            cached.ContentHash = createResponse.Data.ContentHash;
                            cached.LastSyncedAt = DateTime.UtcNow;
                            await _sqlite.SaveRecordAsync(cached);
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    var updateRequest = new UpdateHaircutRecordRequest
                    {
                        Date = request.Date,
                        StylistName = request.StylistName,
                        Location = request.Location,
                        Notes = request.Notes,
                        Price = request.Price,
                        DurationMinutes = request.DurationMinutes,
                        PhotoUrls = request.PhotoUrls
                    };
                    var updateResponse = await _apiService.UpdateHaircutRecordAsync(op.ParentId, op.EntityId, updateRequest);
                    if (updateResponse.Success)
                    {
                        var cached = await _sqlite.GetRecordAsync(op.EntityId);
                        if (cached != null)
                        {
                            cached.SyncStatus = SyncStatus.Synced;
                            cached.ContentHash = updateResponse.Data?.ContentHash ?? cached.ContentHash;
                            cached.LastSyncedAt = DateTime.UtcNow;
                            await _sqlite.SaveRecordAsync(cached);
                        }
                        return true;
                    }
                    return false;
                }

            case OperationType.Delete:
                var deleteResponse = await _apiService.DeleteHaircutRecordAsync(op.ParentId, op.EntityId);
                if (deleteResponse.Success)
                {
                    await _sqlite.DeleteRecordAsync(op.EntityId);
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    private async Task<SyncCompletedEventArgs> DownloadServerChangesAsync()
    {
        var result = new SyncCompletedEventArgs { Success = true };

        try
        {
            // Get sync info from server
            var syncResponse = await _apiService.GetProfileSyncAsync();
            if (!syncResponse.Success || syncResponse.Data == null)
            {
                _logService.Warning("SyncService", $"Failed to get sync info: {syncResponse.Error?.Message}");
                result.Success = false;
                result.ErrorMessage = syncResponse.Error?.Message;
                return result;
            }

            var serverProfiles = syncResponse.Data.Profiles;
            var localProfiles = await _sqlite.GetProfilesAsync();
            var localProfileDict = localProfiles.ToDictionary(p => p.Id);

            // Find profiles to fetch (changed or new)
            var profilesToFetch = new List<string>();
            var profilesToDelete = new List<string>();

            foreach (var serverProfile in serverProfiles)
            {
                if (serverProfile.IsDeleted)
                {
                    if (localProfileDict.ContainsKey(serverProfile.Id))
                    {
                        profilesToDelete.Add(serverProfile.Id);
                    }
                    continue;
                }

                if (!localProfileDict.TryGetValue(serverProfile.Id, out var localProfile))
                {
                    // New profile on server
                    profilesToFetch.Add(serverProfile.Id);
                }
                else if (localProfile.SyncStatus == SyncStatus.Synced &&
                         localProfile.ContentHash != serverProfile.ContentHash)
                {
                    // Profile changed on server (only update if local is synced)
                    profilesToFetch.Add(serverProfile.Id);
                }
            }

            // Find local profiles deleted on server
            var serverIds = serverProfiles.Where(p => !p.IsDeleted).Select(p => p.Id).ToHashSet();
            foreach (var localProfile in localProfiles)
            {
                if (localProfile.SyncStatus == SyncStatus.Synced && !serverIds.Contains(localProfile.Id))
                {
                    profilesToDelete.Add(localProfile.Id);
                }
            }

            _logService.Info("SyncService", $"Sync: {profilesToFetch.Count} to fetch, {profilesToDelete.Count} to delete");

            // Delete removed profiles
            foreach (var id in profilesToDelete)
            {
                await _sqlite.DeleteProfileAsync(id);
                result.ProfilesDeleted++;
            }

            // Fetch changed profiles in batches
            if (profilesToFetch.Count > 0)
            {
                const int batchSize = 20;
                for (var i = 0; i < profilesToFetch.Count; i += batchSize)
                {
                    var batch = profilesToFetch.Skip(i).Take(batchSize).ToList();
                    var batchResponse = await _apiService.GetProfilesBatchAsync(batch);

                    if (batchResponse.Success && batchResponse.Data != null)
                    {
                        foreach (var serverProfile in batchResponse.Data)
                        {
                            var cached = MapToCachedProfile(serverProfile);
                            cached.SyncStatus = SyncStatus.Synced;
                            cached.LastSyncedAt = DateTime.UtcNow;
                            await _sqlite.SaveProfileAsync(cached);
                            result.ProfilesUpdated++;
                        }
                    }
                    else
                    {
                        _logService.Warning("SyncService", $"Failed to fetch batch: {batchResponse.Error?.Message}");
                    }
                }
            }

            await _sqlite.SetLastSyncTimeAsync(syncResponse.Data.ServerTime);
        }
        catch (Exception ex)
        {
            _logService.Error("SyncService", "DownloadServerChanges failed", ex);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private static CachedProfile MapToCachedProfile(Shared.Models.Profile profile)
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
            UpdatedAt = profile.UpdatedAt,
            ContentHash = profile.ContentHash
        };
    }

    /// <summary>
    /// Computes a content hash for a profile.
    /// </summary>
    public static string ComputeContentHash(Profile profile)
    {
        var content = new
        {
            profile.Name,
            profile.Description,
            Measurements = profile.Measurements.Select(m => new { m.Area, m.GuardSize, m.Technique, m.Notes, m.StepOrder }).ToList(),
            profile.AvatarUrl,
            profile.ImageUrl1,
            profile.ImageUrl2,
            profile.ImageUrl3
        };

        var json = JsonSerializer.Serialize(content);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(bytes)[..16];
    }

    /// <summary>
    /// Computes a content hash for a haircut record.
    /// </summary>
    public static string ComputeContentHash(HaircutRecord record)
    {
        var content = new
        {
            record.Date,
            record.StylistName,
            record.Location,
            record.Notes,
            record.Price,
            record.DurationMinutes,
            record.PhotoUrls
        };

        var json = JsonSerializer.Serialize(content);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(bytes)[..16];
    }
}
