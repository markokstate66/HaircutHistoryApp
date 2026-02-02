using HaircutHistoryApp.Shared.DTOs;
using HaircutHistoryApp.Shared.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Interface for Azure Functions API calls.
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Sets the access token for API calls.
    /// </summary>
    void SetAccessToken(string? token);

    // User operations
    Task<ApiResponse<User>> GetCurrentUserAsync();
    Task<ApiResponse<User>> CreateOrUpdateUserAsync(string displayName);

    // Profile operations
    Task<ApiResponse<List<Profile>>> GetProfilesAsync();
    Task<ApiResponse<Profile>> GetProfileAsync(string profileId);
    Task<ApiResponse<Profile>> CreateProfileAsync(string name, string? avatarUrl = null);
    Task<ApiResponse<Profile>> UpdateProfileAsync(string profileId, string? name = null, string? avatarUrl = null);
    Task<ApiResponse> DeleteProfileAsync(string profileId);
    Task<ApiResponse<List<Profile>>> GetSharedProfilesAsync();

    // Haircut record operations
    Task<PaginatedResponse<HaircutRecord>> GetHaircutRecordsAsync(string profileId, int limit = 50, int offset = 0);
    Task<ApiResponse<HaircutRecord>> GetHaircutRecordAsync(string profileId, string recordId);
    Task<ApiResponse<HaircutRecord>> CreateHaircutRecordAsync(string profileId, CreateHaircutRecordRequest request);
    Task<ApiResponse<HaircutRecord>> UpdateHaircutRecordAsync(string profileId, string recordId, UpdateHaircutRecordRequest request);
    Task<ApiResponse> DeleteHaircutRecordAsync(string profileId, string recordId);

    // Share operations
    Task<ApiResponse<ShareToken>> GenerateShareLinkAsync(string profileId);
    Task<ApiResponse<object>> AcceptShareAsync(string token);
    Task<ApiResponse> RevokeShareAsync(string profileId, string stylistUserId);
    Task<ApiResponse<List<ProfileShare>>> GetSharesAsync(string profileId);

    // Photo operations
    Task<ApiResponse<PhotoUploadResponse>> GetPhotoUploadUrlAsync(string fileName, string contentType);
    Task<ApiResponse> DeletePhotoAsync(string blobUrl);
}
