using HaircutHistoryApp.Models;
using Newtonsoft.Json;

namespace HaircutHistoryApp.Services;

public class PlayFabDataService : IDataService
{
    private readonly IPlayFabService _playFabService;
    private readonly ILogService _log;
    private const string ProfilesKey = "haircut_profiles";
    private const string ShareSessionsKey = "share_sessions";
    private const string Tag = "PlayFabDataService";

    public PlayFabDataService(IPlayFabService playFabService, ILogService logService)
    {
        _playFabService = playFabService;
        _log = logService;
    }

    #region Profile Operations

    public async Task<List<HaircutProfile>> GetProfilesAsync(string userId)
    {
        var json = await _playFabService.GetPlayerDataAsync(ProfilesKey);

        if (string.IsNullOrEmpty(json))
            return new List<HaircutProfile>();

        try
        {
            var profiles = JsonConvert.DeserializeObject<List<HaircutProfile>>(json);
            return profiles?.Where(p => p.UserId == userId).OrderByDescending(p => p.UpdatedAt).ToList()
                   ?? new List<HaircutProfile>();
        }
        catch (Exception ex)
        {
            _log.Error("Failed to deserialize profiles", Tag, ex);
            return new List<HaircutProfile>();
        }
    }

    public async Task<HaircutProfile?> GetProfileAsync(string profileId)
    {
        var json = await _playFabService.GetPlayerDataAsync(ProfilesKey);

        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            var profiles = JsonConvert.DeserializeObject<List<HaircutProfile>>(json);
            return profiles?.FirstOrDefault(p => p.Id == profileId);
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to get profile {profileId}", Tag, ex);
            return null;
        }
    }

    public async Task<bool> SaveProfileAsync(HaircutProfile profile)
    {
        try
        {
            var profiles = await GetAllProfilesAsync();
            var index = profiles.FindIndex(p => p.Id == profile.Id);
            var isNew = index < 0;

            profile.UpdatedAt = DateTime.UtcNow;

            if (index >= 0)
            {
                profiles[index] = profile;
            }
            else
            {
                profiles.Add(profile);
            }

            var json = JsonConvert.SerializeObject(profiles);
            var success = await _playFabService.SavePlayerDataAsync(ProfilesKey, json);

            if (success && isNew)
            {
                // Track haircut creation and check achievements
                await _playFabService.IncrementStatAndCheckAchievementsAsync(PlayFabConfig.Statistics.HaircutsCreated);
            }

            return success;
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to save profile {profile.Id}", Tag, ex);
            return false;
        }
    }

    public async Task<bool> DeleteProfileAsync(string profileId)
    {
        try
        {
            var profiles = await GetAllProfilesAsync();
            profiles.RemoveAll(p => p.Id == profileId);

            var json = JsonConvert.SerializeObject(profiles);
            return await _playFabService.SavePlayerDataAsync(ProfilesKey, json);
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to delete profile {profileId}", Tag, ex);
            return false;
        }
    }

    private async Task<List<HaircutProfile>> GetAllProfilesAsync()
    {
        var json = await _playFabService.GetPlayerDataAsync(ProfilesKey);

        if (string.IsNullOrEmpty(json))
            return new List<HaircutProfile>();

        try
        {
            return JsonConvert.DeserializeObject<List<HaircutProfile>>(json) ?? new List<HaircutProfile>();
        }
        catch (Exception ex)
        {
            _log.Error("Failed to deserialize all profiles", Tag, ex);
            return new List<HaircutProfile>();
        }
    }

    #endregion

    #region Share Session Operations

    public async Task<ShareSession> CreateShareSessionAsync(string profileId, string clientUserId, string clientName, bool allowNotes)
    {
        var session = new ShareSession
        {
            ProfileId = profileId,
            ClientUserId = clientUserId,
            ClientName = clientName,
            AllowBarberNotes = allowNotes,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        var sessions = await GetAllSessionsAsync();
        sessions.RemoveAll(s => s.ProfileId == profileId && s.ClientUserId == clientUserId);
        sessions.Add(session);

        var json = JsonConvert.SerializeObject(sessions);
        await _playFabService.SavePlayerDataAsync(ShareSessionsKey, json);

        // Track sharing and check achievements
        await _playFabService.IncrementStatAndCheckAchievementsAsync(PlayFabConfig.Statistics.ProfilesShared);

        return session;
    }

    public async Task<ShareSession?> GetShareSessionAsync(string sessionId)
    {
        var sessions = await GetAllSessionsAsync();
        var session = sessions.FirstOrDefault(s => s.Id == sessionId);

        if (session != null && session.IsExpired)
            return null;

        return session;
    }

    public async Task<(HaircutProfile? Profile, ShareSession? Session)> GetSharedProfileAsync(string sessionId)
    {
        var session = await GetShareSessionAsync(sessionId);
        if (session == null)
            return (null, null);

        var profile = await GetProfileAsync(session.ProfileId);
        return (profile, session);
    }

    private async Task<List<ShareSession>> GetAllSessionsAsync()
    {
        var json = await _playFabService.GetPlayerDataAsync(ShareSessionsKey);

        if (string.IsNullOrEmpty(json))
            return new List<ShareSession>();

        try
        {
            var sessions = JsonConvert.DeserializeObject<List<ShareSession>>(json) ?? new List<ShareSession>();
            sessions.RemoveAll(s => s.IsExpired);
            return sessions;
        }
        catch (Exception ex)
        {
            _log.Error("Failed to deserialize share sessions", Tag, ex);
            return new List<ShareSession>();
        }
    }

    #endregion

    #region Barber Notes

    public async Task<bool> AddBarberNoteAsync(string profileId, BarberNote note)
    {
        var profile = await GetProfileAsync(profileId);
        if (profile == null)
            return false;

        profile.BarberNotes.Add(note);
        var success = await SaveProfileAsync(profile);

        if (success)
        {
            // Track notes and check achievements
            await _playFabService.IncrementStatAndCheckAchievementsAsync(PlayFabConfig.Statistics.NotesAdded);
        }

        return success;
    }

    #endregion

    #region Recent Clients

    public async Task<List<RecentClient>> GetRecentClientsAsync(string barberId)
    {
        var key = $"recent_clients_{barberId}";
        var json = await _playFabService.GetPlayerDataAsync(key);

        if (string.IsNullOrEmpty(json))
            return new List<RecentClient>();

        try
        {
            var clients = JsonConvert.DeserializeObject<List<RecentClient>>(json) ?? new List<RecentClient>();
            return clients.OrderByDescending(c => c.ViewedAt).Take(20).ToList();
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to get recent clients for barber {barberId}", Tag, ex);
            return new List<RecentClient>();
        }
    }

    public async Task SaveRecentClientAsync(string barberId, RecentClient client)
    {
        var clients = await GetRecentClientsAsync(barberId);
        clients.RemoveAll(c => c.SessionId == client.SessionId);
        clients.Insert(0, client);

        if (clients.Count > 20)
            clients = clients.Take(20).ToList();

        var key = $"recent_clients_{barberId}";
        var json = JsonConvert.SerializeObject(clients);
        await _playFabService.SavePlayerDataAsync(key, json);

        // Track client views and check achievements
        await _playFabService.IncrementStatAndCheckAchievementsAsync(PlayFabConfig.Statistics.ClientsViewed);
    }

    #endregion
}
