using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

public interface IPlayFabService
{
    bool IsLoggedIn { get; }
    string? PlayFabId { get; }

    // Authentication
    Task<(bool Success, string? Error)> RegisterAsync(string email, string password, string displayName);
    Task<(bool Success, string? Error)> LoginAsync(string email, string password);
    Task<(bool Success, string? Error)> LoginWithDeviceAsync();
    Task LogoutAsync();

    // Player Data (for haircut profiles)
    Task<bool> SavePlayerDataAsync(string key, string jsonData);
    Task<string?> GetPlayerDataAsync(string key);
    Task<Dictionary<string, string>> GetAllPlayerDataAsync();
    Task<bool> DeletePlayerDataAsync(string key);

    // Statistics & Achievements
    Task<bool> UpdateStatisticAsync(string statName, int value);
    Task<bool> IncrementStatisticAsync(string statName, int incrementBy = 1);
    Task<Dictionary<string, int>> GetStatisticsAsync();
    Task<List<Achievement>> GetAchievementsAsync(bool isBarberMode);
    Task<List<Achievement>> CheckAndUnlockAchievementsAsync(string statName, int newValue);
    Task<List<Achievement>> IncrementStatAndCheckAchievementsAsync(string statName, int incrementBy = 1);
    Task<List<Achievement>> RecordBarberVisitAsync();

    // Leaderboards (optional future feature)
    Task<bool> SubmitScoreAsync(string leaderboardName, int score);
}
