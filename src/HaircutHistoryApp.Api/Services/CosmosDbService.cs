using HaircutHistoryApp.Shared.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using User = HaircutHistoryApp.Shared.Models.User;

namespace HaircutHistoryApp.Api.Services;

/// <summary>
/// Implementation of Cosmos DB operations.
/// </summary>
public class CosmosDbService : ICosmosDbService
{
    private readonly Container _usersContainer;
    private readonly Container _profilesContainer;
    private readonly Container _haircutRecordsContainer;
    private readonly Container _profileSharesContainer;

    public CosmosDbService(CosmosClient client, string databaseName)
    {
        var database = client.GetDatabase(databaseName);
        _usersContainer = database.GetContainer("Users");
        _profilesContainer = database.GetContainer("Profiles");
        _haircutRecordsContainer = database.GetContainer("HaircutRecords");
        _profileSharesContainer = database.GetContainer("ProfileShares");
    }

    #region User Operations

    public async Task<User?> GetUserAsync(string userId)
    {
        try
        {
            var response = await _usersContainer.ReadItemAsync<User>(userId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<User> UpsertUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var response = await _usersContainer.UpsertItemAsync(user, new PartitionKey(user.Id));
        return response.Resource;
    }

    #endregion

    #region Profile Operations

    public async Task<List<Profile>> GetProfilesByOwnerAsync(string ownerUserId)
    {
        var query = _profilesContainer.GetItemLinqQueryable<Profile>()
            .Where(p => p.OwnerUserId == ownerUserId && !p.IsDeleted);

        var profiles = new List<Profile>();
        using var iterator = query.ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            profiles.AddRange(response);
        }

        // Get haircut counts for each profile
        foreach (var profile in profiles)
        {
            profile.HaircutCount = await GetHaircutRecordCountAsync(profile.Id);
        }

        return profiles;
    }

    public async Task<Profile?> GetProfileAsync(string profileId, string ownerUserId)
    {
        try
        {
            var response = await _profilesContainer.ReadItemAsync<Profile>(profileId, new PartitionKey(ownerUserId));
            var profile = response.Resource;
            if (profile.IsDeleted) return null;
            profile.HaircutCount = await GetHaircutRecordCountAsync(profileId);
            return profile;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Profile?> GetProfileByIdAsync(string profileId)
    {
        // Cross-partition query to find profile by ID
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @profileId AND c.isDeleted = false")
            .WithParameter("@profileId", profileId);

        using var iterator = _profilesContainer.GetItemQueryIterator<Profile>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var profile = response.FirstOrDefault();
            if (profile != null)
            {
                profile.HaircutCount = await GetHaircutRecordCountAsync(profileId);
                return profile;
            }
        }

        return null;
    }

    public async Task<Profile> CreateProfileAsync(Profile profile)
    {
        profile.CreatedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;
        var response = await _profilesContainer.CreateItemAsync(profile, new PartitionKey(profile.OwnerUserId));
        return response.Resource;
    }

    public async Task<Profile> UpdateProfileAsync(Profile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        var response = await _profilesContainer.ReplaceItemAsync(profile, profile.Id, new PartitionKey(profile.OwnerUserId));
        return response.Resource;
    }

    public async Task DeleteProfileAsync(string profileId, string ownerUserId)
    {
        var profile = await GetProfileAsync(profileId, ownerUserId);
        if (profile != null)
        {
            profile.IsDeleted = true;
            profile.UpdatedAt = DateTime.UtcNow;
            await _profilesContainer.ReplaceItemAsync(profile, profileId, new PartitionKey(ownerUserId));
        }
    }

    public async Task<int> GetProfileCountAsync(string ownerUserId)
    {
        var query = _profilesContainer.GetItemLinqQueryable<Profile>()
            .Where(p => p.OwnerUserId == ownerUserId && !p.IsDeleted)
            .Select(p => 1);

        var count = 0;
        using var iterator = query.ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            count += response.Count;
        }

        return count;
    }

    #endregion

    #region Haircut Record Operations

    public async Task<List<HaircutRecord>> GetHaircutRecordsAsync(string profileId, int limit = 50, int offset = 0)
    {
        var query = _haircutRecordsContainer.GetItemLinqQueryable<HaircutRecord>()
            .Where(r => r.ProfileId == profileId && !r.IsDeleted)
            .OrderByDescending(r => r.Date)
            .Skip(offset)
            .Take(limit);

        var records = new List<HaircutRecord>();
        using var iterator = query.ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            records.AddRange(response);
        }

        return records;
    }

    public async Task<HaircutRecord?> GetHaircutRecordAsync(string recordId, string profileId)
    {
        try
        {
            var response = await _haircutRecordsContainer.ReadItemAsync<HaircutRecord>(recordId, new PartitionKey(profileId));
            var record = response.Resource;
            return record.IsDeleted ? null : record;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<HaircutRecord> CreateHaircutRecordAsync(HaircutRecord record)
    {
        record.CreatedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;
        var response = await _haircutRecordsContainer.CreateItemAsync(record, new PartitionKey(record.ProfileId));
        return response.Resource;
    }

    public async Task<HaircutRecord> UpdateHaircutRecordAsync(HaircutRecord record)
    {
        record.UpdatedAt = DateTime.UtcNow;
        var response = await _haircutRecordsContainer.ReplaceItemAsync(record, record.Id, new PartitionKey(record.ProfileId));
        return response.Resource;
    }

    public async Task DeleteHaircutRecordAsync(string recordId, string profileId)
    {
        var record = await GetHaircutRecordAsync(recordId, profileId);
        if (record != null)
        {
            record.IsDeleted = true;
            record.UpdatedAt = DateTime.UtcNow;
            await _haircutRecordsContainer.ReplaceItemAsync(record, recordId, new PartitionKey(profileId));
        }
    }

    public async Task<int> GetHaircutRecordCountAsync(string profileId)
    {
        var query = _haircutRecordsContainer.GetItemLinqQueryable<HaircutRecord>()
            .Where(r => r.ProfileId == profileId && !r.IsDeleted)
            .Select(r => 1);

        var count = 0;
        using var iterator = query.ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            count += response.Count;
        }

        return count;
    }

    #endregion

    #region Share Operations

    public async Task<List<ProfileShare>> GetSharesByProfileAsync(string profileId)
    {
        // Query across all partitions since we need all stylists for a profile
        var query = new QueryDefinition("SELECT * FROM c WHERE c.profileId = @profileId AND c.isActive = true")
            .WithParameter("@profileId", profileId);

        var shares = new List<ProfileShare>();
        using var iterator = _profileSharesContainer.GetItemQueryIterator<ProfileShare>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            shares.AddRange(response);
        }

        return shares;
    }

    public async Task<List<ProfileShare>> GetSharesByStylistAsync(string stylistUserId)
    {
        var query = _profileSharesContainer.GetItemLinqQueryable<ProfileShare>()
            .Where(s => s.StylistUserId == stylistUserId && s.IsActive);

        var shares = new List<ProfileShare>();
        using var iterator = query.ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            shares.AddRange(response);
        }

        return shares;
    }

    public async Task<ProfileShare?> GetShareAsync(string profileId, string stylistUserId)
    {
        var query = _profileSharesContainer.GetItemLinqQueryable<ProfileShare>()
            .Where(s => s.ProfileId == profileId && s.StylistUserId == stylistUserId && s.IsActive);

        using var iterator = query.ToFeedIterator();

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<ProfileShare> CreateShareAsync(ProfileShare share)
    {
        share.SharedAt = DateTime.UtcNow;
        var response = await _profileSharesContainer.CreateItemAsync(share, new PartitionKey(share.StylistUserId));
        return response.Resource;
    }

    public async Task RevokeShareAsync(string profileId, string stylistUserId)
    {
        var share = await GetShareAsync(profileId, stylistUserId);
        if (share != null)
        {
            share.IsActive = false;
            share.RevokedAt = DateTime.UtcNow;
            await _profileSharesContainer.ReplaceItemAsync(share, share.Id, new PartitionKey(share.StylistUserId));
        }
    }

    public async Task<bool> HasAccessToProfileAsync(string profileId, string userId)
    {
        // Check if user owns the profile
        var query = _profilesContainer.GetItemLinqQueryable<Profile>()
            .Where(p => p.Id == profileId && p.OwnerUserId == userId && !p.IsDeleted);

        using var ownerIterator = query.ToFeedIterator();
        if (ownerIterator.HasMoreResults)
        {
            var ownerResponse = await ownerIterator.ReadNextAsync();
            if (ownerResponse.Any()) return true;
        }

        // Check if profile is shared with user
        var share = await GetShareAsync(profileId, userId);
        return share != null;
    }

    public async Task<List<Profile>> GetSharedProfilesAsync(string stylistUserId)
    {
        var shares = await GetSharesByStylistAsync(stylistUserId);
        var profiles = new List<Profile>();

        foreach (var share in shares)
        {
            // We need to find the profile, but we don't know the ownerUserId
            // Use a cross-partition query
            var profileQuery = new QueryDefinition("SELECT * FROM c WHERE c.id = @profileId AND c.isDeleted = false")
                .WithParameter("@profileId", share.ProfileId);

            using var iterator = _profilesContainer.GetItemQueryIterator<Profile>(profileQuery);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var profile = response.FirstOrDefault();
                if (profile != null)
                {
                    profile.HaircutCount = await GetHaircutRecordCountAsync(profile.Id);
                    profiles.Add(profile);
                }
            }
        }

        return profiles;
    }

    #endregion

    #region Sync Operations

    public async Task<List<Profile>> GetProfilesSyncInfoAsync(string ownerUserId)
    {
        // Return lightweight profile info for sync comparison
        var query = _profilesContainer.GetItemLinqQueryable<Profile>()
            .Where(p => p.OwnerUserId == ownerUserId);

        var profiles = new List<Profile>();
        using var iterator = query.ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            profiles.AddRange(response);
        }

        return profiles;
    }

    public async Task<List<Profile>> GetProfilesByIdsAsync(List<string> profileIds, string ownerUserId)
    {
        if (profileIds.Count == 0)
            return new List<Profile>();

        var profiles = new List<Profile>();

        foreach (var id in profileIds)
        {
            var profile = await GetProfileAsync(id, ownerUserId);
            if (profile != null)
            {
                profiles.Add(profile);
            }
        }

        return profiles;
    }

    #endregion
}
