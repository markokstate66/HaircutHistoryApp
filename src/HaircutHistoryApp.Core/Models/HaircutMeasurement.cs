namespace HaircutHistoryApp.Core.Models;

public class HaircutMeasurement
{
    public string Area { get; set; } = string.Empty;
    public string GuardSize { get; set; } = string.Empty;
    public string Technique { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string DisplayText
    {
        get
        {
            var guardDisplay = string.IsNullOrEmpty(GuardSize) ? "" : $"#{GuardSize}";
            var techniqueDisplay = string.IsNullOrEmpty(Technique) ? "" : $" ({Technique})";
            return $"{Area}: {guardDisplay}{techniqueDisplay}";
        }
    }

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
        "0",
        "0.5",
        "1",
        "1.5",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "Scissors",
        "Finger length",
        "Custom"
    };

    public static List<string> CommonTechniques => new()
    {
        "Fade",
        "Taper",
        "Blend",
        "Scissor cut",
        "Clipper over comb",
        "Texturize",
        "Layer",
        "Thin out",
        "Square off",
        "Round off"
    };
}
