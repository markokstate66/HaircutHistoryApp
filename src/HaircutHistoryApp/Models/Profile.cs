namespace HaircutHistoryApp.Models;

/// <summary>
/// Represents a haircut template/recipe (e.g., "Dad's winter haircut", "Ryder's summer cut").
/// Contains the master measurements that define this haircut style.
/// </summary>
public class Profile
{
    public string Id { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<HaircutMeasurement> Measurements { get; set; } = new();
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Reference photo URLs (up to 3 images)
    /// </summary>
    public string? ImageUrl1 { get; set; }
    public string? ImageUrl2 { get; set; }
    public string? ImageUrl3 { get; set; }

    /// <summary>
    /// Returns true if any reference images are set
    /// </summary>
    public bool HasImages => !string.IsNullOrEmpty(ImageUrl1) ||
                             !string.IsNullOrEmpty(ImageUrl2) ||
                             !string.IsNullOrEmpty(ImageUrl3);
    public int HaircutCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets initials from the name for avatar placeholder display.
    /// </summary>
    public string Initials => string.IsNullOrEmpty(Name) ? "?"
        : string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Where(w => w.Length > 0)
            .Select(w => char.ToUpper(w[0])));

    /// <summary>
    /// Display text showing haircut count.
    /// </summary>
    public string HaircutCountDisplay => HaircutCount switch
    {
        0 => "No haircuts yet",
        1 => "1 haircut",
        _ => $"{HaircutCount} haircuts"
    };

    /// <summary>
    /// Summary of key measurements for display.
    /// </summary>
    public string MeasurementsSummary
    {
        get
        {
            if (Measurements.Count == 0)
                return "No measurements set";

            var top = Measurements.FirstOrDefault(m => m.Area == "Top");
            var sides = Measurements.FirstOrDefault(m => m.Area == "Sides");
            var parts = new List<string>();

            if (top != null && !string.IsNullOrEmpty(top.GuardSize))
                parts.Add($"Top: {top.GuardSize}");
            if (sides != null && !string.IsNullOrEmpty(sides.GuardSize))
                parts.Add($"Sides: {sides.GuardSize}");

            return parts.Count > 0 ? string.Join(" | ", parts) : "Custom measurements";
        }
    }
}
