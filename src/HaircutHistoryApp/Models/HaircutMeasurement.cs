namespace HaircutHistoryApp.Models;

public class HaircutMeasurement
{
    public string Area { get; set; } = string.Empty;
    public string GuardSize { get; set; } = string.Empty;
    public string Technique { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public static List<string> CommonAreas => new()
    {
        "Top",
        "Sides",
        "Back",
        "Neckline",
        "Sideburns",
        "Bangs/Fringe",
        "Crown",
        "Beard",
        "Mustache"
    };

    public static List<string> CommonGuardSizes => new()
    {
        "0 (1/16\")",
        "1 (1/8\")",
        "2 (1/4\")",
        "3 (3/8\")",
        "4 (1/2\")",
        "5 (5/8\")",
        "6 (3/4\")",
        "7 (7/8\")",
        "8 (1\")",
        "Scissors",
        "Razor",
        "Finger Length",
        "Custom"
    };

    public static List<string> CommonTechniques => new()
    {
        "Fade",
        "Taper",
        "Blended",
        "Textured",
        "Layered",
        "Point Cut",
        "Razor Cut",
        "Clipper Over Comb",
        "Scissors Over Comb",
        "Undercut",
        "Disconnected",
        "Lined Up",
        "Natural"
    };
}
