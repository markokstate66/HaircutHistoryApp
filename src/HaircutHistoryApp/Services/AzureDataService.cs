using HaircutHistoryApp.Models;
using HaircutHistoryApp.Shared.DTOs;
using HaircutHistoryApp.Shared.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Data service implementation using Azure Functions API.
/// Maps between local MAUI models and shared API models.
/// </summary>
public class AzureDataService : IDataService
{
    private readonly IApiService _apiService;
    private readonly ILogService _logService;

    public string? LastError { get; private set; }

    public AzureDataService(IApiService apiService, ILogService logService)
    {
        _apiService = apiService;
        _logService = logService;
    }

    #region Profile Operations

    public async Task<List<Models.Profile>> GetProfilesAsync()
    {
        LastError = null;
        try
        {
            var response = await _apiService.GetProfilesAsync();
            if (!response.Success || response.Data == null)
            {
                LastError = response.Error?.Message ?? "Failed to get profiles";
                _logService.Warning("AzureDataService", LastError);
                return new List<Models.Profile>();
            }

            return response.Data.Select(MapToLocalProfile).ToList();
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "GetProfilesAsync failed", ex);
            return new List<Models.Profile>();
        }
    }

    public async Task<Models.Profile?> GetProfileAsync(string profileId)
    {
        LastError = null;
        try
        {
            var response = await _apiService.GetProfileAsync(profileId);
            if (!response.Success || response.Data == null)
            {
                LastError = response.Error?.Message;
                return null;
            }

            return MapToLocalProfile(response.Data);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "GetProfileAsync failed", ex);
            return null;
        }
    }

    public async Task<bool> SaveProfileAsync(Models.Profile profile)
    {
        LastError = null;
        try
        {
            if (string.IsNullOrEmpty(profile.Id))
            {
                // Create new profile
                _logService.Info("AzureDataService", $"Creating profile: {profile.Name}");
                var request = new CreateProfileRequest
                {
                    Name = profile.Name,
                    Description = profile.Description,
                    Measurements = profile.Measurements.Select(m => new Measurement
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
                var response = await _apiService.CreateProfileAsync(request);

                if (!response.Success || response.Data == null)
                {
                    LastError = response.Error?.Message ?? "Failed to create profile";
                    _logService.Error("AzureDataService", LastError);
                    return false;
                }

                profile.Id = response.Data.Id;
                profile.CreatedAt = response.Data.CreatedAt;
                profile.UpdatedAt = response.Data.UpdatedAt;
                return true;
            }
            else
            {
                // Update existing profile
                _logService.Info("AzureDataService", $"Updating profile: {profile.Id}");
                var request = new UpdateProfileRequest
                {
                    Name = profile.Name,
                    Description = profile.Description,
                    Measurements = profile.Measurements.Select(m => new Measurement
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
                var response = await _apiService.UpdateProfileAsync(profile.Id, request);

                if (!response.Success)
                {
                    LastError = response.Error?.Message ?? "Failed to update profile";
                    _logService.Error("AzureDataService", LastError);
                    return false;
                }

                if (response.Data != null)
                {
                    profile.UpdatedAt = response.Data.UpdatedAt;
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "SaveProfileAsync failed", ex);
            return false;
        }
    }

    public async Task<bool> DeleteProfileAsync(string profileId)
    {
        LastError = null;
        try
        {
            var response = await _apiService.DeleteProfileAsync(profileId);
            if (!response.Success)
            {
                LastError = response.Error?.Message ?? "Failed to delete profile";
            }
            return response.Success;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "DeleteProfileAsync failed", ex);
            return false;
        }
    }

    #endregion

    #region Haircut Record Operations

    public async Task<List<Models.HaircutRecord>> GetHaircutRecordsAsync(string profileId)
    {
        LastError = null;
        try
        {
            var response = await _apiService.GetHaircutRecordsAsync(profileId);
            // PaginatedResponse directly contains Data, no Success/Error wrapper
            if (response.Data == null)
            {
                return new List<Models.HaircutRecord>();
            }

            return response.Data.Select(MapToLocalRecord).ToList();
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "GetHaircutRecordsAsync failed", ex);
            return new List<Models.HaircutRecord>();
        }
    }

    public async Task<Models.HaircutRecord?> GetHaircutRecordAsync(string profileId, string recordId)
    {
        LastError = null;
        try
        {
            var response = await _apiService.GetHaircutRecordAsync(profileId, recordId);
            if (!response.Success || response.Data == null)
            {
                LastError = response.Error?.Message;
                return null;
            }

            return MapToLocalRecord(response.Data);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "GetHaircutRecordAsync failed", ex);
            return null;
        }
    }

    public async Task<bool> SaveHaircutRecordAsync(string profileId, Models.HaircutRecord record)
    {
        LastError = null;
        try
        {
            if (string.IsNullOrEmpty(record.Id))
            {
                // Create new record
                _logService.Info("AzureDataService", $"Creating haircut record for profile: {profileId}");
                var request = MapToCreateRequest(record);
                var response = await _apiService.CreateHaircutRecordAsync(profileId, request);

                if (!response.Success || response.Data == null)
                {
                    LastError = response.Error?.Message ?? "Failed to create haircut record";
                    _logService.Error("AzureDataService", LastError);
                    return false;
                }

                record.Id = response.Data.Id;
                record.ProfileId = response.Data.ProfileId;
                record.CreatedByUserId = response.Data.CreatedByUserId;
                record.CreatedAt = response.Data.CreatedAt;
                record.UpdatedAt = response.Data.UpdatedAt;
                return true;
            }
            else
            {
                // Update existing record
                _logService.Info("AzureDataService", $"Updating haircut record: {record.Id}");
                var request = MapToUpdateRequest(record);
                var response = await _apiService.UpdateHaircutRecordAsync(profileId, record.Id, request);

                if (!response.Success)
                {
                    LastError = response.Error?.Message ?? "Failed to update haircut record";
                    _logService.Error("AzureDataService", LastError);
                    return false;
                }

                if (response.Data != null)
                {
                    record.UpdatedAt = response.Data.UpdatedAt;
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "SaveHaircutRecordAsync failed", ex);
            return false;
        }
    }

    public async Task<bool> DeleteHaircutRecordAsync(string profileId, string recordId)
    {
        LastError = null;
        try
        {
            var response = await _apiService.DeleteHaircutRecordAsync(profileId, recordId);
            if (!response.Success)
            {
                LastError = response.Error?.Message ?? "Failed to delete haircut record";
            }
            return response.Success;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "DeleteHaircutRecordAsync failed", ex);
            return false;
        }
    }

    #endregion

    #region Share Operations

    public async Task<ShareSession> CreateShareSessionAsync(string profileId)
    {
        LastError = null;
        try
        {
            var response = await _apiService.GenerateShareLinkAsync(profileId);
            if (!response.Success || response.Data == null)
            {
                LastError = response.Error?.Message ?? "Failed to create share session";
                // Return a placeholder session
                return new ShareSession
                {
                    Id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    ProfileId = profileId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }

            return new ShareSession
            {
                Id = ExtractTokenId(response.Data.Token),
                ProfileId = profileId,
                Token = response.Data.Token,
                ShareUrl = response.Data.ShareUrl,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = response.Data.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "CreateShareSessionAsync failed", ex);
            return new ShareSession
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                ProfileId = profileId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }
    }

    public async Task<bool> AcceptShareAsync(string token)
    {
        LastError = null;
        try
        {
            var response = await _apiService.AcceptShareAsync(token);
            if (!response.Success)
            {
                LastError = response.Error?.Message ?? "Failed to accept share";
            }
            return response.Success;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "AcceptShareAsync failed", ex);
            return false;
        }
    }

    public async Task<List<Models.Profile>> GetSharedProfilesAsync()
    {
        LastError = null;
        try
        {
            var response = await _apiService.GetSharedProfilesAsync();
            if (!response.Success || response.Data == null)
            {
                LastError = response.Error?.Message ?? "Failed to get shared profiles";
                _logService.Warning("AzureDataService", LastError);
                return new List<Models.Profile>();
            }

            return response.Data.Select(MapToLocalProfile).ToList();
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            _logService.Error("AzureDataService", "GetSharedProfilesAsync failed", ex);
            return new List<Models.Profile>();
        }
    }

    #endregion

    #region Mapping Helpers

    private Models.Profile MapToLocalProfile(Shared.Models.Profile p) => new()
    {
        Id = p.Id,
        OwnerUserId = p.OwnerUserId,
        Name = p.Name,
        Description = p.Description,
        Measurements = p.Measurements.Select(m => new HaircutMeasurement
        {
            Area = m.Area,
            GuardSize = m.GuardSize,
            Technique = m.Technique,
            Notes = m.Notes,
            StepOrder = m.StepOrder
        }).OrderBy(m => m.StepOrder).ToList(),
        AvatarUrl = p.AvatarUrl,
        ImageUrl1 = p.ImageUrl1,
        ImageUrl2 = p.ImageUrl2,
        ImageUrl3 = p.ImageUrl3,
        HaircutCount = p.HaircutCount,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private Models.HaircutRecord MapToLocalRecord(Shared.Models.HaircutRecord r) => new()
    {
        Id = r.Id,
        ProfileId = r.ProfileId,
        CreatedByUserId = r.CreatedByUserId,
        Date = r.Date,
        StylistName = r.StylistName,
        Location = r.Location,
        PhotoUrls = r.PhotoUrls.ToList(),
        Notes = r.Notes,
        Price = r.Price,
        DurationMinutes = r.DurationMinutes,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };

    private CreateHaircutRecordRequest MapToCreateRequest(Models.HaircutRecord record) => new()
    {
        Date = record.Date,
        StylistName = record.StylistName,
        Location = record.Location,
        Notes = record.Notes,
        Price = record.Price,
        DurationMinutes = record.DurationMinutes,
        PhotoUrls = record.PhotoUrls.ToList()
    };

    private UpdateHaircutRecordRequest MapToUpdateRequest(Models.HaircutRecord record) => new()
    {
        Date = record.Date,
        StylistName = record.StylistName,
        Location = record.Location,
        Notes = record.Notes,
        Price = record.Price,
        DurationMinutes = record.DurationMinutes,
        PhotoUrls = record.PhotoUrls.ToList()
    };

    private string ExtractTokenId(string token)
    {
        // Return first 8 characters of the token as a readable ID
        if (token.Length >= 8)
            return token.Substring(0, 8).ToUpper();
        return token.ToUpper();
    }

    #endregion
}
