using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.Models;

/// <summary>
/// Represents a measurement for a specific area of a haircut.
/// </summary>
public class Measurement
{
    /// <summary>
    /// The area of the head (e.g., "Top", "Sides", "Back").
    /// </summary>
    [JsonPropertyName("area")]
    public string Area { get; set; } = string.Empty;

    /// <summary>
    /// The guard size used (e.g., "2", "3", "Scissors").
    /// </summary>
    [JsonPropertyName("guardSize")]
    public string GuardSize { get; set; } = string.Empty;

    /// <summary>
    /// The technique used (e.g., "Fade", "Taper", "Blend").
    /// </summary>
    [JsonPropertyName("technique")]
    public string Technique { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes for this measurement.
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// The order/step number for this measurement in the haircut workflow.
    /// Lower numbers are done first. 0 means no specific order.
    /// </summary>
    [JsonPropertyName("stepOrder")]
    public int StepOrder { get; set; }

    /// <summary>
    /// Formatted display text for the measurement.
    /// </summary>
    [JsonIgnore]
    public string DisplayText
    {
        get
        {
            var guardDisplay = string.IsNullOrEmpty(GuardSize) ? "" : $"#{GuardSize}";
            var techniqueDisplay = string.IsNullOrEmpty(Technique) ? "" : $" ({Technique})";
            return $"{Area}: {guardDisplay}{techniqueDisplay}";
        }
    }

    /// <summary>
    /// Common areas of the head for haircut measurements.
    /// </summary>
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

    /// <summary>
    /// Common guard sizes for clippers.
    /// </summary>
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

    /// <summary>
    /// Common haircut techniques.
    /// </summary>
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
