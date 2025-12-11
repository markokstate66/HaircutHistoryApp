namespace HaircutHistoryApp.Services.Analytics;

public interface IAnalyticsService
{
    void Initialize();
    void TrackEvent(string eventName, Dictionary<string, object>? properties = null);
    void TrackScreen(string screenName);
    void SetUserId(string userId);
    void SetUserProperty(string name, string value);
}

public static class AnalyticsEvents
{
    // Authentication
    public const string Login = "login";
    public const string Logout = "logout";
    public const string SignUp = "sign_up";

    // Profile Events
    public const string ProfileCreated = "profile_created";
    public const string ProfileUpdated = "profile_updated";
    public const string ProfileDeleted = "profile_deleted";
    public const string ProfileViewed = "profile_viewed";

    // Measurement Events
    public const string MeasurementAdded = "measurement_added";
    public const string MeasurementUpdated = "measurement_updated";

    // Photo Events
    public const string PhotoAdded = "photo_added";
    public const string PhotoRemoved = "photo_removed";

    // Sharing Events
    public const string QRCodeGenerated = "qr_code_generated";
    public const string QRCodeScanned = "qr_code_scanned";
    public const string ProfileShared = "profile_shared";

    // Barber Events
    public const string BarberNoteAdded = "barber_note_added";
    public const string ClientViewed = "client_viewed";

    // Mode Events
    public const string ModeSwitched = "mode_switched";

    // Achievement Events
    public const string AchievementUnlocked = "achievement_unlocked";
    public const string AchievementsViewed = "achievements_viewed";
}

public static class AnalyticsProperties
{
    public const string ProfileId = "profile_id";
    public const string ProfileName = "profile_name";
    public const string MeasurementArea = "measurement_area";
    public const string GuardSize = "guard_size";
    public const string Technique = "technique";
    public const string PhotoCount = "photo_count";
    public const string ShareSessionId = "share_session_id";
    public const string AchievementId = "achievement_id";
    public const string AchievementName = "achievement_name";
    public const string UserMode = "user_mode";
    public const string Method = "method";
}

public static class AnalyticsScreens
{
    public const string Home = "Home";
    public const string ProfileList = "ProfileList";
    public const string ProfileDetail = "ProfileDetail";
    public const string ProfileEdit = "ProfileEdit";
    public const string AddMeasurement = "AddMeasurement";
    public const string QRCode = "QRCode";
    public const string QRScanner = "QRScanner";
    public const string ClientView = "ClientView";
    public const string Achievements = "Achievements";
    public const string Settings = "Settings";
    public const string Login = "Login";
    public const string Register = "Register";
}
