using CommunityToolkit.Mvvm.ComponentModel;

namespace HaircutHistoryApp.Models;

public partial class HaircutMeasurement : ObservableObject
{
    [ObservableProperty]
    private string _area = string.Empty;

    [ObservableProperty]
    private string _guardSize = string.Empty;

    [ObservableProperty]
    private string _technique = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    /// <summary>
    /// The order/step number for this measurement in the haircut workflow.
    /// Lower numbers are done first.
    /// </summary>
    [ObservableProperty]
    private int _stepOrder;

    /// <summary>
    /// Display text showing the step number and area.
    /// </summary>
    public string StepDisplay => StepOrder > 0 ? $"Step {StepOrder}: {Area}" : Area;

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
