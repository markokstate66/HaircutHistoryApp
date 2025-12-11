namespace HaircutHistoryApp.Services;

public static class PlayFabConfig
{
    // PlayFab Title ID from Game Manager
    public const string TitleId = "1F53F6";

    // Statistics keys for tracking progress
    public static class Statistics
    {
        // Client stats
        public const string HaircutsCreated = "haircuts_created";
        public const string BarberVisits = "barber_visits";      // Completed via barber scan
        public const string ProfilesShared = "profiles_shared";

        // Barber stats
        public const string ClientsViewed = "clients_viewed";
        public const string NotesAdded = "notes_added";
    }

    // Achievement thresholds for reference
    public static class Thresholds
    {
        public static readonly int[] Haircuts = { 1, 5, 10, 25, 50, 100 };
        public static readonly int[] BarberVisits = { 1, 5, 10, 25, 50, 100 };
        public static readonly int[] Shares = { 1, 10, 50 };
        public static readonly int[] Clients = { 1, 10, 50, 100 };
        public static readonly int[] Notes = { 1, 25, 100 };
    }
}
