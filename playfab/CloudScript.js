// HaircutHistoryApp - PlayFab CloudScript
// Upload this to PlayFab Game Manager -> Automation -> CloudScript

// ============================================
// ACHIEVEMENT SYSTEM
// ============================================

handlers.CheckAchievements = function (args, context) {
    var statName = args.statName;
    var newValue = args.newValue;

    var achievementIds = getAchievementIdsForStat(statName, newValue);

    if (achievementIds.length === 0) {
        return { unlocked: [], message: "No new achievements" };
    }

    // Get currently unlocked achievements
    var playerData = server.GetUserData({
        PlayFabId: currentPlayerId,
        Keys: ["unlocked_achievements"]
    });

    var unlockedList = [];
    if (playerData.Data.unlocked_achievements) {
        try {
            unlockedList = JSON.parse(playerData.Data.unlocked_achievements.Value);
        } catch (e) {
            unlockedList = [];
        }
    }

    var newlyUnlocked = [];
    for (var i = 0; i < achievementIds.length; i++) {
        var id = achievementIds[i];
        if (unlockedList.indexOf(id) === -1) {
            unlockedList.push(id);
            newlyUnlocked.push(id);
        }
    }

    if (newlyUnlocked.length > 0) {
        server.UpdateUserData({
            PlayFabId: currentPlayerId,
            Data: {
                "unlocked_achievements": JSON.stringify(unlockedList)
            }
        });
    }

    return {
        unlocked: newlyUnlocked,
        totalUnlocked: unlockedList.length,
        message: newlyUnlocked.length > 0 ? "Achievements unlocked!" : "No new achievements"
    };
};

function getAchievementIdsForStat(statName, value) {
    var ids = [];

    switch (statName) {
        case "haircuts_created":
            if (value >= 1) ids.push("HAIRCUT_1");
            if (value >= 5) ids.push("HAIRCUT_5");
            if (value >= 10) ids.push("HAIRCUT_10");
            if (value >= 25) ids.push("HAIRCUT_25");
            if (value >= 50) ids.push("HAIRCUT_50");
            if (value >= 100) ids.push("HAIRCUT_100");
            break;

        case "barber_visits":
            if (value >= 1) ids.push("VISIT_1");
            if (value >= 5) ids.push("VISIT_5");
            if (value >= 10) ids.push("VISIT_10");
            if (value >= 25) ids.push("VISIT_25");
            if (value >= 50) ids.push("VISIT_50");
            if (value >= 100) ids.push("VISIT_100");
            break;

        case "profiles_shared":
            if (value >= 1) ids.push("SHARE_1");
            if (value >= 10) ids.push("SHARE_10");
            if (value >= 50) ids.push("SHARE_50");
            break;

        case "clients_viewed":
            if (value >= 1) ids.push("CLIENT_1");
            if (value >= 10) ids.push("CLIENT_10");
            if (value >= 50) ids.push("CLIENT_50");
            if (value >= 100) ids.push("CLIENT_100");
            break;

        case "notes_added":
            if (value >= 1) ids.push("NOTE_1");
            if (value >= 25) ids.push("NOTE_25");
            if (value >= 100) ids.push("NOTE_100");
            break;
    }

    return ids;
}

// ============================================
// PROFILE STATISTICS
// ============================================

handlers.IncrementStatistic = function (args, context) {
    var statName = args.statName;
    var incrementBy = args.incrementBy || 1;

    // Get current value
    var stats = server.GetPlayerStatistics({
        PlayFabId: currentPlayerId,
        StatisticNames: [statName]
    });

    var currentValue = 0;
    if (stats.Statistics && stats.Statistics.length > 0) {
        currentValue = stats.Statistics[0].Value;
    }

    var newValue = currentValue + incrementBy;

    // Update statistic
    server.UpdatePlayerStatistics({
        PlayFabId: currentPlayerId,
        Statistics: [
            { StatisticName: statName, Value: newValue }
        ]
    });

    // Check for achievements
    var achievementResult = handlers.CheckAchievements({
        statName: statName,
        newValue: newValue
    }, context);

    return {
        success: true,
        statName: statName,
        previousValue: currentValue,
        newValue: newValue,
        achievements: achievementResult.unlocked
    };
};

// ============================================
// BARBER VISIT (Special action - completed haircut)
// ============================================

handlers.RecordBarberVisit = function (args, context) {
    return handlers.IncrementStatistic({
        statName: "barber_visits",
        incrementBy: 1
    }, context);
};

// ============================================
// GET PLAYER STATS
// ============================================

handlers.GetPlayerStats = function (args, context) {
    var statNames = [
        "haircuts_created",
        "barber_visits",
        "profiles_shared",
        "clients_viewed",
        "notes_added"
    ];

    var stats = server.GetPlayerStatistics({
        PlayFabId: currentPlayerId,
        StatisticNames: statNames
    });

    var result = {};
    if (stats.Statistics) {
        for (var i = 0; i < stats.Statistics.length; i++) {
            result[stats.Statistics[i].StatisticName] = stats.Statistics[i].Value;
        }
    }

    // Fill in zeros for missing stats
    for (var j = 0; j < statNames.length; j++) {
        if (!result[statNames[j]]) {
            result[statNames[j]] = 0;
        }
    }

    return result;
};

// ============================================
// GET ACHIEVEMENTS
// ============================================

handlers.GetAchievements = function (args, context) {
    var isBarberMode = args.isBarberMode || false;

    // Get unlocked achievements
    var playerData = server.GetUserData({
        PlayFabId: currentPlayerId,
        Keys: ["unlocked_achievements"]
    });

    var unlockedList = [];
    if (playerData.Data.unlocked_achievements) {
        try {
            unlockedList = JSON.parse(playerData.Data.unlocked_achievements.Value);
        } catch (e) {
            unlockedList = [];
        }
    }

    // Get stats for progress
    var stats = handlers.GetPlayerStats({}, context);

    // Define achievements (client vs barber)
    var achievements = isBarberMode ? getBarberAchievements() : getClientAchievements();

    // Map progress
    for (var i = 0; i < achievements.length; i++) {
        var a = achievements[i];
        a.isUnlocked = unlockedList.indexOf(a.id) !== -1;
        a.currentValue = getStatValueForAchievement(a.id, stats);
    }

    return {
        achievements: achievements,
        unlockedCount: unlockedList.length,
        totalCount: achievements.length
    };
};

function getClientAchievements() {
    return [
        // Haircuts
        { id: "HAIRCUT_1", name: "First Cut", description: "Create your first haircut profile", icon: "✂️", targetValue: 1, category: "Haircuts" },
        { id: "HAIRCUT_5", name: "Style Explorer", description: "Create 5 haircut profiles", icon: "💇", targetValue: 5, category: "Haircuts" },
        { id: "HAIRCUT_10", name: "Style Collector", description: "Create 10 haircut profiles", icon: "📚", targetValue: 10, category: "Haircuts" },
        { id: "HAIRCUT_25", name: "Style Enthusiast", description: "Create 25 haircut profiles", icon: "🎨", targetValue: 25, category: "Haircuts" },
        { id: "HAIRCUT_50", name: "Style Master", description: "Create 50 haircut profiles", icon: "🏆", targetValue: 50, category: "Haircuts" },
        { id: "HAIRCUT_100", name: "Style Legend", description: "Create 100 haircut profiles", icon: "👑", targetValue: 100, category: "Haircuts" },

        // Barber Visits
        { id: "VISIT_1", name: "Fresh Cut", description: "Complete your first barber visit", icon: "💈", targetValue: 1, category: "BarberVisits" },
        { id: "VISIT_5", name: "Regular", description: "Complete 5 barber visits", icon: "🪑", targetValue: 5, category: "BarberVisits" },
        { id: "VISIT_10", name: "Loyal Customer", description: "Complete 10 barber visits", icon: "⭐", targetValue: 10, category: "BarberVisits" },
        { id: "VISIT_25", name: "VIP Client", description: "Complete 25 barber visits", icon: "🌟", targetValue: 25, category: "BarberVisits" },
        { id: "VISIT_50", name: "Barbershop Regular", description: "Complete 50 barber visits", icon: "💎", targetValue: 50, category: "BarberVisits" },
        { id: "VISIT_100", name: "Lifetime Member", description: "Complete 100 barber visits", icon: "🏅", targetValue: 100, category: "BarberVisits" },

        // Sharing
        { id: "SHARE_1", name: "First Share", description: "Share a profile with a barber", icon: "📲", targetValue: 1, category: "Sharing" },
        { id: "SHARE_10", name: "Connector", description: "Share profiles 10 times", icon: "🔗", targetValue: 10, category: "Sharing" },
        { id: "SHARE_50", name: "Networker", description: "Share profiles 50 times", icon: "🌐", targetValue: 50, category: "Sharing" }
    ];
}

function getBarberAchievements() {
    return [
        { id: "CLIENT_1", name: "First Client", description: "View your first client's profile", icon: "👋", targetValue: 1, category: "BarberMode" },
        { id: "CLIENT_10", name: "Growing Clientele", description: "View 10 client profiles", icon: "📈", targetValue: 10, category: "BarberMode" },
        { id: "CLIENT_50", name: "Busy Barber", description: "View 50 client profiles", icon: "🔥", targetValue: 50, category: "BarberMode" },
        { id: "CLIENT_100", name: "Master Barber", description: "View 100 client profiles", icon: "🏆", targetValue: 100, category: "BarberMode" },
        { id: "NOTE_1", name: "Helpful Tip", description: "Add your first note to a client", icon: "📝", targetValue: 1, category: "BarberMode" },
        { id: "NOTE_25", name: "Note Taker", description: "Add 25 notes to clients", icon: "✍️", targetValue: 25, category: "BarberMode" },
        { id: "NOTE_100", name: "Detail Oriented", description: "Add 100 notes to clients", icon: "🎓", targetValue: 100, category: "BarberMode" }
    ];
}

function getStatValueForAchievement(achievementId, stats) {
    if (achievementId.indexOf("HAIRCUT") === 0) return stats.haircuts_created || 0;
    if (achievementId.indexOf("VISIT") === 0) return stats.barber_visits || 0;
    if (achievementId.indexOf("SHARE") === 0) return stats.profiles_shared || 0;
    if (achievementId.indexOf("CLIENT") === 0) return stats.clients_viewed || 0;
    if (achievementId.indexOf("NOTE") === 0) return stats.notes_added || 0;
    return 0;
}
