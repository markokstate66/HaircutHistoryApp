namespace HaircutHistoryApp.Models;

public class Achievement
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public AchievementCategory Category { get; set; }

    public double Progress => TargetValue > 0 ? Math.Min((double)CurrentValue / TargetValue, 1.0) : 0;
    public string ProgressText => $"{CurrentValue}/{TargetValue}";
}

public enum AchievementCategory
{
    Haircuts,       // Creating profiles
    BarberVisits,   // Completed haircuts via barber scan
    Sharing,        // QR shares
    BarberMode      // For barbers
}

public static class AchievementDefinitions
{
    public static List<Achievement> GetAll() => new()
    {
        // === HAIRCUT PROFILES (Creating profiles) ===
        new Achievement
        {
            Id = "HAIRCUT_1",
            Name = "First Cut",
            Description = "Create your first haircut profile",
            Icon = "✂️",
            TargetValue = 1,
            Category = AchievementCategory.Haircuts
        },
        new Achievement
        {
            Id = "HAIRCUT_5",
            Name = "Style Explorer",
            Description = "Create 5 haircut profiles",
            Icon = "💇",
            TargetValue = 5,
            Category = AchievementCategory.Haircuts
        },
        new Achievement
        {
            Id = "HAIRCUT_10",
            Name = "Style Collector",
            Description = "Create 10 haircut profiles",
            Icon = "📚",
            TargetValue = 10,
            Category = AchievementCategory.Haircuts
        },
        new Achievement
        {
            Id = "HAIRCUT_25",
            Name = "Style Enthusiast",
            Description = "Create 25 haircut profiles",
            Icon = "🎨",
            TargetValue = 25,
            Category = AchievementCategory.Haircuts
        },
        new Achievement
        {
            Id = "HAIRCUT_50",
            Name = "Style Master",
            Description = "Create 50 haircut profiles",
            Icon = "🏆",
            TargetValue = 50,
            Category = AchievementCategory.Haircuts
        },
        new Achievement
        {
            Id = "HAIRCUT_100",
            Name = "Style Legend",
            Description = "Create 100 haircut profiles",
            Icon = "👑",
            TargetValue = 100,
            Category = AchievementCategory.Haircuts
        },

        // === BARBER VISITS (Completed via barber pairing) ===
        new Achievement
        {
            Id = "VISIT_1",
            Name = "Fresh Cut",
            Description = "Complete your first barber visit",
            Icon = "💈",
            TargetValue = 1,
            Category = AchievementCategory.BarberVisits
        },
        new Achievement
        {
            Id = "VISIT_5",
            Name = "Regular",
            Description = "Complete 5 barber visits",
            Icon = "🪑",
            TargetValue = 5,
            Category = AchievementCategory.BarberVisits
        },
        new Achievement
        {
            Id = "VISIT_10",
            Name = "Loyal Customer",
            Description = "Complete 10 barber visits",
            Icon = "⭐",
            TargetValue = 10,
            Category = AchievementCategory.BarberVisits
        },
        new Achievement
        {
            Id = "VISIT_25",
            Name = "VIP Client",
            Description = "Complete 25 barber visits",
            Icon = "🌟",
            TargetValue = 25,
            Category = AchievementCategory.BarberVisits
        },
        new Achievement
        {
            Id = "VISIT_50",
            Name = "Barbershop Regular",
            Description = "Complete 50 barber visits",
            Icon = "💎",
            TargetValue = 50,
            Category = AchievementCategory.BarberVisits
        },
        new Achievement
        {
            Id = "VISIT_100",
            Name = "Lifetime Member",
            Description = "Complete 100 barber visits",
            Icon = "🏅",
            TargetValue = 100,
            Category = AchievementCategory.BarberVisits
        },

        // === SHARING (QR code shares) ===
        new Achievement
        {
            Id = "SHARE_1",
            Name = "First Share",
            Description = "Share a profile with a barber",
            Icon = "📲",
            TargetValue = 1,
            Category = AchievementCategory.Sharing
        },
        new Achievement
        {
            Id = "SHARE_10",
            Name = "Connector",
            Description = "Share profiles 10 times",
            Icon = "🔗",
            TargetValue = 10,
            Category = AchievementCategory.Sharing
        },
        new Achievement
        {
            Id = "SHARE_50",
            Name = "Networker",
            Description = "Share profiles 50 times",
            Icon = "🌐",
            TargetValue = 50,
            Category = AchievementCategory.Sharing
        },

        // === BARBER MODE (For stylists/barbers) ===
        new Achievement
        {
            Id = "CLIENT_1",
            Name = "First Client",
            Description = "View your first client's profile",
            Icon = "👋",
            TargetValue = 1,
            Category = AchievementCategory.BarberMode
        },
        new Achievement
        {
            Id = "CLIENT_10",
            Name = "Growing Clientele",
            Description = "View 10 client profiles",
            Icon = "📈",
            TargetValue = 10,
            Category = AchievementCategory.BarberMode
        },
        new Achievement
        {
            Id = "CLIENT_50",
            Name = "Busy Barber",
            Description = "View 50 client profiles",
            Icon = "🔥",
            TargetValue = 50,
            Category = AchievementCategory.BarberMode
        },
        new Achievement
        {
            Id = "CLIENT_100",
            Name = "Master Barber",
            Description = "View 100 client profiles",
            Icon = "🏆",
            TargetValue = 100,
            Category = AchievementCategory.BarberMode
        },
        new Achievement
        {
            Id = "NOTE_1",
            Name = "Helpful Tip",
            Description = "Add your first note to a client",
            Icon = "📝",
            TargetValue = 1,
            Category = AchievementCategory.BarberMode
        },
        new Achievement
        {
            Id = "NOTE_25",
            Name = "Note Taker",
            Description = "Add 25 notes to clients",
            Icon = "✍️",
            TargetValue = 25,
            Category = AchievementCategory.BarberMode
        },
        new Achievement
        {
            Id = "NOTE_100",
            Name = "Detail Oriented",
            Description = "Add 100 notes to clients",
            Icon = "🎓",
            TargetValue = 100,
            Category = AchievementCategory.BarberMode
        }
    };

    public static List<Achievement> GetClientAchievements() =>
        GetAll().Where(a => a.Category != AchievementCategory.BarberMode).ToList();

    public static List<Achievement> GetBarberAchievements() =>
        GetAll().Where(a => a.Category == AchievementCategory.BarberMode).ToList();
}
