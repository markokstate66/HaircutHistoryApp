using HaircutHistoryApp.Models;
using Newtonsoft.Json;

namespace HaircutHistoryApp.Services;

public class LocalDataService : IDataService
{
    private const string ProfilesKey = "haircut_profiles";
    private const string ShareSessionsKey = "share_sessions";
    private const string RecentClientsKey = "recent_clients_{0}";

    public async Task<List<HaircutProfile>> GetProfilesAsync(string userId)
    {
        var profiles = await GetAllProfilesAsync();
        return profiles.Where(p => p.UserId == userId).OrderByDescending(p => p.UpdatedAt).ToList();
    }

    public async Task<HaircutProfile?> GetProfileAsync(string profileId)
    {
        var profiles = await GetAllProfilesAsync();
        return profiles.FirstOrDefault(p => p.Id == profileId);
    }

    public async Task<bool> SaveProfileAsync(HaircutProfile profile)
    {
        try
        {
            var profiles = await GetAllProfilesAsync();
            var index = profiles.FindIndex(p => p.Id == profile.Id);

            profile.UpdatedAt = DateTime.UtcNow;

            if (index >= 0)
            {
                profiles[index] = profile;
            }
            else
            {
                profiles.Add(profile);
            }

            await SaveAllProfilesAsync(profiles);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteProfileAsync(string profileId)
    {
        try
        {
            var profiles = await GetAllProfilesAsync();
            profiles.RemoveAll(p => p.Id == profileId);
            await SaveAllProfilesAsync(profiles);
            return true;
        }
        catch
        {
            return false;
        }
    }

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

        // Remove old sessions for this profile
        sessions.RemoveAll(s => s.ProfileId == profileId && s.ClientUserId == clientUserId);

        sessions.Add(session);
        await SaveAllSessionsAsync(sessions);

        return session;
    }

    public async Task<ShareSession?> GetShareSessionAsync(string sessionId)
    {
        var sessions = await GetAllSessionsAsync();
        var session = sessions.FirstOrDefault(s => s.Id == sessionId);

        if (session != null && session.IsExpired)
        {
            return null;
        }

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

    public async Task<bool> AddBarberNoteAsync(string profileId, BarberNote note)
    {
        var profile = await GetProfileAsync(profileId);
        if (profile == null)
            return false;

        profile.BarberNotes.Add(note);
        return await SaveProfileAsync(profile);
    }

    public async Task<List<RecentClient>> GetRecentClientsAsync(string barberId)
    {
        var key = string.Format(RecentClientsKey, barberId);
        var json = Preferences.Get(key, null);

        if (string.IsNullOrEmpty(json))
            return new List<RecentClient>();

        var clients = JsonConvert.DeserializeObject<List<RecentClient>>(json) ?? new List<RecentClient>();
        return await Task.FromResult(clients.OrderByDescending(c => c.ViewedAt).Take(20).ToList());
    }

    public async Task SaveRecentClientAsync(string barberId, RecentClient client)
    {
        var clients = await GetRecentClientsAsync(barberId);

        // Remove existing entry for same session
        clients.RemoveAll(c => c.SessionId == client.SessionId);

        clients.Insert(0, client);

        // Keep only last 20
        if (clients.Count > 20)
            clients = clients.Take(20).ToList();

        var key = string.Format(RecentClientsKey, barberId);
        var json = JsonConvert.SerializeObject(clients);
        Preferences.Set(key, json);
    }

    private Task<List<HaircutProfile>> GetAllProfilesAsync()
    {
        var json = Preferences.Get(ProfilesKey, null);
        if (string.IsNullOrEmpty(json))
            return Task.FromResult(new List<HaircutProfile>());

        return Task.FromResult(JsonConvert.DeserializeObject<List<HaircutProfile>>(json) ?? new List<HaircutProfile>());
    }

    private Task SaveAllProfilesAsync(List<HaircutProfile> profiles)
    {
        var json = JsonConvert.SerializeObject(profiles);
        Preferences.Set(ProfilesKey, json);
        return Task.CompletedTask;
    }

    private Task<List<ShareSession>> GetAllSessionsAsync()
    {
        var json = Preferences.Get(ShareSessionsKey, null);
        if (string.IsNullOrEmpty(json))
            return Task.FromResult(new List<ShareSession>());

        var sessions = JsonConvert.DeserializeObject<List<ShareSession>>(json) ?? new List<ShareSession>();

        // Clean up expired sessions
        sessions.RemoveAll(s => s.IsExpired);

        return Task.FromResult(sessions);
    }

    private Task SaveAllSessionsAsync(List<ShareSession> sessions)
    {
        var json = JsonConvert.SerializeObject(sessions);
        Preferences.Set(ShareSessionsKey, json);
        return Task.CompletedTask;
    }
}
