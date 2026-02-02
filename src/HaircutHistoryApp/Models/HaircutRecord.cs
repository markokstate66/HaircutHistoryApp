namespace HaircutHistoryApp.Models;

/// <summary>
/// Represents a single haircut event for a profile.
/// Contains all the details: measurements, photos, stylist info, etc.
/// </summary>
public class HaircutRecord
{
    public string Id { get; set; } = string.Empty;
    public string ProfileId { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    public string? StylistName { get; set; }
    public string? Location { get; set; }
    public List<HaircutMeasurement> Measurements { get; set; } = new();
    public List<string> PhotoUrls { get; set; } = new();
    public List<string> Products { get; set; } = new();
    public string? Notes { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Display summary for list views.
    /// </summary>
    public string DisplaySummary => !string.IsNullOrEmpty(Description)
        ? Description
        : MeasurementsSummary;

    /// <summary>
    /// Summary of key measurements (Top and Sides).
    /// </summary>
    public string MeasurementsSummary
    {
        get
        {
            if (Measurements.Count == 0)
                return $"Haircut on {Date:MMM d, yyyy}";

            var top = Measurements.FirstOrDefault(m => m.Area == "Top");
            var sides = Measurements.FirstOrDefault(m => m.Area == "Sides");
            var parts = new List<string>();

            if (top != null && !string.IsNullOrEmpty(top.GuardSize))
                parts.Add($"Top: {top.GuardSize}");
            if (sides != null && !string.IsNullOrEmpty(sides.GuardSize))
                parts.Add($"Sides: {sides.GuardSize}");

            return parts.Count > 0 ? string.Join(" | ", parts) : $"Haircut on {Date:MMM d, yyyy}";
        }
    }

    /// <summary>
    /// Formatted date display.
    /// </summary>
    public string DateDisplay => Date.ToString("MMM d, yyyy");

    /// <summary>
    /// Day of month for calendar-style display.
    /// </summary>
    public string DayDisplay => Date.Day.ToString();

    /// <summary>
    /// Month abbreviation for calendar-style display.
    /// </summary>
    public string MonthDisplay => Date.ToString("MMM").ToUpper();

    /// <summary>
    /// Formatted price display.
    /// </summary>
    public string? PriceDisplay => Price.HasValue ? $"${Price:F2}" : null;

    /// <summary>
    /// Formatted duration display.
    /// </summary>
    public string? DurationDisplay => DurationMinutes.HasValue
        ? DurationMinutes.Value >= 60
            ? $"{DurationMinutes.Value / 60}h {DurationMinutes.Value % 60}m"
            : $"{DurationMinutes.Value} min"
        : null;
}
