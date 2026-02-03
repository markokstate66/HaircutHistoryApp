namespace HaircutHistoryApp.Models;

/// <summary>
/// Represents a log entry of when a haircut profile was used.
/// This is a simple record - measurements live on the Profile.
/// </summary>
public class HaircutRecord
{
    public string Id { get; set; } = string.Empty;
    public string ProfileId { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public string? StylistName { get; set; }
    public string? Location { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
    public string? Notes { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Display summary for list views.
    /// </summary>
    public string DisplaySummary => !string.IsNullOrEmpty(Notes)
        ? Notes
        : $"Haircut on {Date:MMM d, yyyy}";

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
