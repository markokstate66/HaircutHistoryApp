using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

public interface IDataService
{
    // Profile operations
    Task<List<HaircutProfile>> GetProfilesAsync(string userId);
    Task<HaircutProfile?> GetProfileAsync(string profileId);
    Task<bool> SaveProfileAsync(HaircutProfile profile);
    Task<bool> DeleteProfileAsync(string profileId);

    // Share session operations
    Task<ShareSession> CreateShareSessionAsync(string profileId, string clientUserId, string clientName, bool allowNotes);
    Task<ShareSession?> GetShareSessionAsync(string sessionId);
    Task<(HaircutProfile? Profile, ShareSession? Session)> GetSharedProfileAsync(string sessionId);

    // Barber notes
    Task<bool> AddBarberNoteAsync(string profileId, BarberNote note);

    // Recent clients (for barber mode)
    Task<List<RecentClient>> GetRecentClientsAsync(string barberId);
    Task SaveRecentClientAsync(string barberId, RecentClient client);
}
