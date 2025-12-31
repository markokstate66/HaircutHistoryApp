using HaircutHistoryApp.Models;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;

namespace HaircutHistoryApp.Services;

public class PlayFabService : IPlayFabService
{
    private string? _playFabId;
    private string? _sessionTicket;

    private const string SessionTicketKey = "playfab_session_ticket";
    private const string PlayFabIdKey = "playfab_id";
    private const string AuthMethodKey = "auth_method";

    public bool IsLoggedIn => !string.IsNullOrEmpty(_sessionTicket);
    public string? PlayFabId => _playFabId;

    public PlayFabService()
    {
        PlayFabSettings.staticSettings.TitleId = PlayFabConfig.TitleId;
    }

    /// <summary>
    /// Try to restore a previous session on app startup
    /// </summary>
    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            var authMethod = await SecureStorage.GetAsync(AuthMethodKey);
            if (string.IsNullOrEmpty(authMethod))
                return false;

            // Try device login to restore session
            var (success, _) = await LoginWithDeviceAsync();
            return success;
        }
        catch
        {
            return false;
        }
    }

    private async Task SaveSessionAsync(string authMethod)
    {
        try
        {
            await SecureStorage.SetAsync(SessionTicketKey, _sessionTicket ?? "");
            await SecureStorage.SetAsync(PlayFabIdKey, _playFabId ?? "");
            await SecureStorage.SetAsync(AuthMethodKey, authMethod);
        }
        catch
        {
            // Ignore storage errors
        }
    }

    private async Task ClearSessionAsync()
    {
        try
        {
            SecureStorage.Remove(SessionTicketKey);
            SecureStorage.Remove(PlayFabIdKey);
            SecureStorage.Remove(AuthMethodKey);
        }
        catch
        {
            // Ignore storage errors
        }
        await Task.CompletedTask;
    }

    #region Authentication

    public async Task<(bool Success, string? Error)> RegisterAsync(string email, string password, string displayName)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            DisplayName = displayName,
            RequireBothUsernameAndEmail = false
        };

        var result = await PlayFabClientAPI.RegisterPlayFabUserAsync(request);

        if (result.Error != null)
        {
            return (false, result.Error.ErrorMessage);
        }

        _playFabId = result.Result.PlayFabId;
        _sessionTicket = result.Result.SessionTicket;

        await SaveSessionAsync("email");
        await LinkDeviceToAccountAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerStatistics = true,
                GetUserData = true
            }
        };

        var result = await PlayFabClientAPI.LoginWithEmailAddressAsync(request);

        if (result.Error != null)
        {
            return (false, result.Error.ErrorMessage);
        }

        _playFabId = result.Result.PlayFabId;
        _sessionTicket = result.Result.SessionTicket;

        await SaveSessionAsync("email");
        await LinkDeviceToAccountAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LoginWithDeviceAsync()
    {
        var deviceId = await GetDeviceIdAsync();

#if ANDROID
        var request = new LoginWithAndroidDeviceIDRequest
        {
            AndroidDeviceId = deviceId,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerStatistics = true,
                GetUserData = true
            }
        };
        var result = await PlayFabClientAPI.LoginWithAndroidDeviceIDAsync(request);
#elif IOS
        var request = new LoginWithIOSDeviceIDRequest
        {
            DeviceId = deviceId,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerStatistics = true,
                GetUserData = true
            }
        };
        var result = await PlayFabClientAPI.LoginWithIOSDeviceIDAsync(request);
#else
        var request = new LoginWithCustomIDRequest
        {
            CustomId = deviceId,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerStatistics = true,
                GetUserData = true
            }
        };
        var result = await PlayFabClientAPI.LoginWithCustomIDAsync(request);
#endif

        if (result.Error != null)
        {
            return (false, result.Error.ErrorMessage);
        }

        _playFabId = result.Result.PlayFabId;
        _sessionTicket = result.Result.SessionTicket;

        await SaveSessionAsync("device");

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LoginWithGoogleAsync(string accessToken)
    {
        var request = new LoginWithGoogleAccountRequest
        {
            ServerAuthCode = accessToken,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerStatistics = true,
                GetUserData = true,
                GetPlayerProfile = true
            }
        };

        var result = await PlayFabClientAPI.LoginWithGoogleAccountAsync(request);

        if (result.Error != null)
        {
            return (false, result.Error.ErrorMessage);
        }

        _playFabId = result.Result.PlayFabId;
        _sessionTicket = result.Result.SessionTicket;

        await SaveSessionAsync("google");
        await LinkDeviceToAccountAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LoginWithFacebookAsync(string accessToken)
    {
        var request = new LoginWithFacebookRequest
        {
            AccessToken = accessToken,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerStatistics = true,
                GetUserData = true,
                GetPlayerProfile = true
            }
        };

        var result = await PlayFabClientAPI.LoginWithFacebookAsync(request);

        if (result.Error != null)
        {
            return (false, result.Error.ErrorMessage);
        }

        _playFabId = result.Result.PlayFabId;
        _sessionTicket = result.Result.SessionTicket;

        await SaveSessionAsync("facebook");
        await LinkDeviceToAccountAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> LoginWithAppleAsync(string identityToken)
    {
        var request = new LoginWithAppleRequest
        {
            IdentityToken = identityToken,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerStatistics = true,
                GetUserData = true,
                GetPlayerProfile = true
            }
        };

        var result = await PlayFabClientAPI.LoginWithAppleAsync(request);

        if (result.Error != null)
        {
            return (false, result.Error.ErrorMessage);
        }

        _playFabId = result.Result.PlayFabId;
        _sessionTicket = result.Result.SessionTicket;

        await SaveSessionAsync("apple");
        await LinkDeviceToAccountAsync();

        return (true, null);
    }

    public async Task LogoutAsync()
    {
        _playFabId = null;
        _sessionTicket = null;
        await ClearSessionAsync();
    }

    private async Task<string> GetDeviceIdAsync()
    {
        var deviceId = await SecureStorage.GetAsync("device_id");
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = Guid.NewGuid().ToString();
            await SecureStorage.SetAsync("device_id", deviceId);
        }
        return deviceId;
    }

    /// <summary>
    /// Link current device to the logged-in PlayFab account.
    /// This allows session restoration via device login.
    /// </summary>
    private async Task LinkDeviceToAccountAsync()
    {
        if (!IsLoggedIn) return;

        try
        {
            var deviceId = await GetDeviceIdAsync();

#if ANDROID
            var request = new PlayFab.ClientModels.LinkAndroidDeviceIDRequest
            {
                AndroidDeviceId = deviceId,
                ForceLink = true
            };
            await PlayFabClientAPI.LinkAndroidDeviceIDAsync(request);
#elif IOS
            var request = new PlayFab.ClientModels.LinkIOSDeviceIDRequest
            {
                DeviceId = deviceId,
                ForceLink = true
            };
            await PlayFabClientAPI.LinkIOSDeviceIDAsync(request);
#else
            var request = new PlayFab.ClientModels.LinkCustomIDRequest
            {
                CustomId = deviceId,
                ForceLink = true
            };
            await PlayFabClientAPI.LinkCustomIDAsync(request);
#endif
        }
        catch
        {
            // Device may already be linked, ignore errors
        }
    }

    #endregion

    #region Player Data

    public async Task<bool> SavePlayerDataAsync(string key, string jsonData)
    {
        if (!IsLoggedIn) return false;

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { key, jsonData } }
        };

        var result = await PlayFabClientAPI.UpdateUserDataAsync(request);
        return result.Error == null;
    }

    public async Task<string?> GetPlayerDataAsync(string key)
    {
        if (!IsLoggedIn) return null;

        var request = new GetUserDataRequest
        {
            Keys = new List<string> { key }
        };

        var result = await PlayFabClientAPI.GetUserDataAsync(request);

        if (result.Error != null || result.Result.Data == null)
            return null;

        return result.Result.Data.TryGetValue(key, out var record) ? record.Value : null;
    }

    public async Task<Dictionary<string, string>> GetAllPlayerDataAsync()
    {
        if (!IsLoggedIn) return new Dictionary<string, string>();

        var request = new GetUserDataRequest();
        var result = await PlayFabClientAPI.GetUserDataAsync(request);

        if (result.Error != null || result.Result.Data == null)
            return new Dictionary<string, string>();

        return result.Result.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
    }

    public async Task<bool> DeletePlayerDataAsync(string key)
    {
        if (!IsLoggedIn) return false;

        var request = new UpdateUserDataRequest
        {
            KeysToRemove = new List<string> { key }
        };

        var result = await PlayFabClientAPI.UpdateUserDataAsync(request);
        return result.Error == null;
    }

    #endregion

    #region Statistics & Achievements

    public async Task<bool> UpdateStatisticAsync(string statName, int value)
    {
        if (!IsLoggedIn) return false;

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = statName, Value = value }
            }
        };

        var result = await PlayFabClientAPI.UpdatePlayerStatisticsAsync(request);
        return result.Error == null;
    }

    public async Task<bool> IncrementStatisticAsync(string statName, int incrementBy = 1)
    {
        var stats = await GetStatisticsAsync();
        var currentValue = stats.TryGetValue(statName, out var val) ? val : 0;
        return await UpdateStatisticAsync(statName, currentValue + incrementBy);
    }

    public async Task<Dictionary<string, int>> GetStatisticsAsync()
    {
        if (!IsLoggedIn) return new Dictionary<string, int>();

        var request = new GetPlayerStatisticsRequest();
        var result = await PlayFabClientAPI.GetPlayerStatisticsAsync(request);

        if (result.Error != null || result.Result.Statistics == null)
            return new Dictionary<string, int>();

        return result.Result.Statistics.ToDictionary(s => s.StatisticName, s => s.Value);
    }

    public async Task<List<Achievement>> GetAchievementsAsync(bool isBarberMode)
    {
        var achievements = isBarberMode
            ? AchievementDefinitions.GetBarberAchievements()
            : AchievementDefinitions.GetClientAchievements();

        var stats = await GetStatisticsAsync();
        var unlockedData = await GetPlayerDataAsync("unlocked_achievements");
        var unlockedSet = new HashSet<string>();

        if (!string.IsNullOrEmpty(unlockedData))
        {
            try
            {
                var unlockedList = JsonConvert.DeserializeObject<List<string>>(unlockedData);
                if (unlockedList != null)
                    unlockedSet = unlockedList.ToHashSet();
            }
            catch { }
        }

        foreach (var achievement in achievements)
        {
            achievement.IsUnlocked = unlockedSet.Contains(achievement.Id);

            // Map achievement to appropriate stat based on category
            achievement.CurrentValue = achievement.Category switch
            {
                AchievementCategory.Haircuts =>
                    stats.GetValueOrDefault(PlayFabConfig.Statistics.HaircutsCreated, 0),
                AchievementCategory.BarberVisits =>
                    stats.GetValueOrDefault(PlayFabConfig.Statistics.BarberVisits, 0),
                AchievementCategory.Sharing =>
                    stats.GetValueOrDefault(PlayFabConfig.Statistics.ProfilesShared, 0),
                AchievementCategory.BarberMode when achievement.Id.StartsWith("CLIENT") =>
                    stats.GetValueOrDefault(PlayFabConfig.Statistics.ClientsViewed, 0),
                AchievementCategory.BarberMode when achievement.Id.StartsWith("NOTE") =>
                    stats.GetValueOrDefault(PlayFabConfig.Statistics.NotesAdded, 0),
                _ => 0
            };

            // Cap at target if unlocked
            if (achievement.IsUnlocked && achievement.CurrentValue < achievement.TargetValue)
            {
                achievement.CurrentValue = achievement.TargetValue;
            }
        }

        return achievements;
    }

    public async Task<List<Achievement>> CheckAndUnlockAchievementsAsync(string statName, int newValue)
    {
        var unlockedAchievements = new List<Achievement>();

        // Get achievement IDs that should be unlocked based on the stat
        var achievementIds = GetAchievementIdsForStat(statName, newValue);

        if (!achievementIds.Any())
            return unlockedAchievements;

        // Get currently unlocked achievements
        var unlockedData = await GetPlayerDataAsync("unlocked_achievements");
        var unlockedList = new List<string>();

        if (!string.IsNullOrEmpty(unlockedData))
        {
            try
            {
                unlockedList = JsonConvert.DeserializeObject<List<string>>(unlockedData) ?? new List<string>();
            }
            catch { }
        }

        var allAchievements = AchievementDefinitions.GetAll();
        var newlyUnlocked = false;

        foreach (var id in achievementIds)
        {
            if (!unlockedList.Contains(id))
            {
                unlockedList.Add(id);
                newlyUnlocked = true;

                var achievement = allAchievements.FirstOrDefault(a => a.Id == id);
                if (achievement != null)
                {
                    achievement.IsUnlocked = true;
                    achievement.UnlockedAt = DateTime.UtcNow;
                    achievement.CurrentValue = achievement.TargetValue;
                    unlockedAchievements.Add(achievement);
                }
            }
        }

        if (newlyUnlocked)
        {
            await SavePlayerDataAsync("unlocked_achievements", JsonConvert.SerializeObject(unlockedList));
        }

        return unlockedAchievements;
    }

    private List<string> GetAchievementIdsForStat(string statName, int value)
    {
        var ids = new List<string>();

        switch (statName)
        {
            case PlayFabConfig.Statistics.HaircutsCreated:
                if (value >= 1) ids.Add("HAIRCUT_1");
                if (value >= 5) ids.Add("HAIRCUT_5");
                if (value >= 10) ids.Add("HAIRCUT_10");
                if (value >= 25) ids.Add("HAIRCUT_25");
                if (value >= 50) ids.Add("HAIRCUT_50");
                if (value >= 100) ids.Add("HAIRCUT_100");
                break;

            case PlayFabConfig.Statistics.BarberVisits:
                if (value >= 1) ids.Add("VISIT_1");
                if (value >= 5) ids.Add("VISIT_5");
                if (value >= 10) ids.Add("VISIT_10");
                if (value >= 25) ids.Add("VISIT_25");
                if (value >= 50) ids.Add("VISIT_50");
                if (value >= 100) ids.Add("VISIT_100");
                break;

            case PlayFabConfig.Statistics.ProfilesShared:
                if (value >= 1) ids.Add("SHARE_1");
                if (value >= 10) ids.Add("SHARE_10");
                if (value >= 50) ids.Add("SHARE_50");
                break;

            case PlayFabConfig.Statistics.ClientsViewed:
                if (value >= 1) ids.Add("CLIENT_1");
                if (value >= 10) ids.Add("CLIENT_10");
                if (value >= 50) ids.Add("CLIENT_50");
                if (value >= 100) ids.Add("CLIENT_100");
                break;

            case PlayFabConfig.Statistics.NotesAdded:
                if (value >= 1) ids.Add("NOTE_1");
                if (value >= 25) ids.Add("NOTE_25");
                if (value >= 100) ids.Add("NOTE_100");
                break;
        }

        return ids;
    }

    /// <summary>
    /// Increment a stat and check for any newly unlocked achievements
    /// </summary>
    public async Task<List<Achievement>> IncrementStatAndCheckAchievementsAsync(string statName, int incrementBy = 1)
    {
        var stats = await GetStatisticsAsync();
        var currentValue = stats.GetValueOrDefault(statName, 0);
        var newValue = currentValue + incrementBy;

        await UpdateStatisticAsync(statName, newValue);

        return await CheckAndUnlockAchievementsAsync(statName, newValue);
    }

    /// <summary>
    /// Record a completed barber visit (when barber scans and confirms)
    /// </summary>
    public async Task<List<Achievement>> RecordBarberVisitAsync()
    {
        return await IncrementStatAndCheckAchievementsAsync(PlayFabConfig.Statistics.BarberVisits);
    }

    #endregion

    #region Leaderboards

    public async Task<bool> SubmitScoreAsync(string leaderboardName, int score)
    {
        return await UpdateStatisticAsync(leaderboardName, score);
    }

    #endregion
}
