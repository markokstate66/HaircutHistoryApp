using PlayFab;
using PlayFab.AdminModels;

const string TitleId = "1F53F6";
const string SecretKey = "TMFGU1PRJNBQ3DBPA5RAQ94E5QWR3JZJBPUAUKTQYOWUFFFTJF";

PlayFabSettings.staticSettings.TitleId = TitleId;
PlayFabSettings.staticSettings.DeveloperSecretKey = SecretKey;

Console.WriteLine("=== HaircutHistoryApp PlayFab Setup ===\n");

// 1. Create Statistics (Leaderboards)
Console.WriteLine("Creating statistics...");

var statistics = new[]
{
    "haircuts_created",
    "barber_visits",
    "profiles_shared",
    "clients_viewed",
    "notes_added"
};

foreach (var stat in statistics)
{
    try
    {
        var request = new CreatePlayerStatisticDefinitionRequest
        {
            StatisticName = stat,
            VersionChangeInterval = StatisticResetIntervalOption.Never,
            AggregationMethod = StatisticAggregationMethod.Last
        };

        var result = await PlayFabAdminAPI.CreatePlayerStatisticDefinitionAsync(request);

        if (result.Error != null)
        {
            if (result.Error.ErrorMessage.Contains("already exists"))
            {
                Console.WriteLine($"  [EXISTS] {stat}");
            }
            else
            {
                Console.WriteLine($"  [ERROR] {stat}: {result.Error.ErrorMessage}");
            }
        }
        else
        {
            Console.WriteLine($"  [CREATED] {stat}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [ERROR] {stat}: {ex.Message}");
    }
}

// 2. Upload CloudScript
Console.WriteLine("\nUploading CloudScript...");

var cloudScriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "CloudScript.js");
if (!File.Exists(cloudScriptPath))
{
    cloudScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "CloudScript.js");
}

if (File.Exists(cloudScriptPath))
{
    var scriptContent = await File.ReadAllTextAsync(cloudScriptPath);

    var cloudScriptRequest = new UpdateCloudScriptRequest
    {
        Files = new List<CloudScriptFile>
        {
            new CloudScriptFile
            {
                Filename = "main.js",
                FileContents = scriptContent
            }
        },
        Publish = true
    };

    var cloudResult = await PlayFabAdminAPI.UpdateCloudScriptAsync(cloudScriptRequest);

    if (cloudResult.Error != null)
    {
        Console.WriteLine($"  [ERROR] CloudScript: {cloudResult.Error.ErrorMessage}");
    }
    else
    {
        Console.WriteLine($"  [UPLOADED] CloudScript (Revision {cloudResult.Result.Revision})");
    }
}
else
{
    Console.WriteLine($"  [SKIP] CloudScript.js not found at {cloudScriptPath}");
}

// 3. Get current API settings
Console.WriteLine("\nChecking API settings...");

var settingsRequest = new GetPolicyRequest();
var settingsResult = await PlayFabAdminAPI.GetPolicyAsync(settingsRequest);

if (settingsResult.Error != null)
{
    Console.WriteLine($"  [INFO] Could not read policies: {settingsResult.Error.ErrorMessage}");
}
else
{
    Console.WriteLine($"  [OK] API policies retrieved");
}

// 4. Summary
Console.WriteLine("\n=== Setup Complete ===");
Console.WriteLine("\nStatistics created:");
foreach (var stat in statistics)
{
    Console.WriteLine($"  - {stat}");
}

Console.WriteLine("\nAchievement thresholds:");
Console.WriteLine("  Haircuts: 1, 5, 10, 25, 50, 100");
Console.WriteLine("  Barber Visits: 1, 5, 10, 25, 50, 100");
Console.WriteLine("  Shares: 1, 10, 50");
Console.WriteLine("  Clients Viewed: 1, 10, 50, 100");
Console.WriteLine("  Notes Added: 1, 25, 100");

Console.WriteLine("\n[IMPORTANT] Manual steps required in PlayFab Game Manager:");
Console.WriteLine("  1. Settings -> API Features -> Enable 'Allow client to post player statistics'");
Console.WriteLine("  2. Add-ons -> Authentication -> Enable 'Email and password'");

Console.WriteLine("\nDone!");
