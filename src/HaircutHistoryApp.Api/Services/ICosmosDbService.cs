using HaircutHistoryApp.Shared.Models;

namespace HaircutHistoryApp.Api.Services;

/// <summary>
/// Service for interacting with Azure Cosmos DB.
/// </summary>
public interface ICosmosDbService
{
    // User operations
    Task<User?> GetUserAsync(string userId);
    Task<User> UpsertUserAsync(User user);

    // Profile operations
    Task<List<Profile>> GetProfilesByOwnerAsync(string ownerUserId);
    Task<Profile?> GetProfileAsync(string profileId, string ownerUserId);
    Task<Profile?> GetProfileByIdAsync(string profileId); // Cross-partition query
    Task<Profile> CreateProfileAsync(Profile profile);
    Task<Profile> UpdateProfileAsync(Profile profile);
    Task DeleteProfileAsync(string profileId, string ownerUserId);
    Task<int> GetProfileCountAsync(string ownerUserId);

    // Haircut record operations
    Task<List<HaircutRecord>> GetHaircutRecordsAsync(string profileId, int limit = 50, int offset = 0);
    Task<HaircutRecord?> GetHaircutRecordAsync(string recordId, string profileId);
    Task<HaircutRecord> CreateHaircutRecordAsync(HaircutRecord record);
    Task<HaircutRecord> UpdateHaircutRecordAsync(HaircutRecord record);
    Task DeleteHaircutRecordAsync(string recordId, string profileId);
    Task<int> GetHaircutRecordCountAsync(string profileId);

    // Share operations
    Task<List<ProfileShare>> GetSharesByProfileAsync(string profileId);
    Task<List<ProfileShare>> GetSharesByStylistAsync(string stylistUserId);
    Task<ProfileShare?> GetShareAsync(string profileId, string stylistUserId);
    Task<ProfileShare> CreateShareAsync(ProfileShare share);
    Task RevokeShareAsync(string profileId, string stylistUserId);
    Task<bool> HasAccessToProfileAsync(string profileId, string userId);

    // Cross-container queries
    Task<List<Profile>> GetSharedProfilesAsync(string stylistUserId);

    // Sync operations
    Task<List<Profile>> GetProfilesSyncInfoAsync(string ownerUserId);
    Task<List<Profile>> GetProfilesByIdsAsync(List<string> profileIds, string ownerUserId);
}
