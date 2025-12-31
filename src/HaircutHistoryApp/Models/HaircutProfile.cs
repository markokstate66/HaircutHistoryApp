namespace HaircutHistoryApp.Models;

public class HaircutProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;  // Who this profile is for (e.g., "Ryder", "Mark")
    public string Description { get; set; } = string.Empty;
    public List<HaircutMeasurement> Measurements { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public List<string> LocalImagePaths { get; set; } = new();
    public string? ThumbnailUrl { get; set; }
    public string GeneralNotes { get; set; } = string.Empty;
    public List<BarberNote> BarberNotes { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsFavorite { get; set; } = false;

    public string DisplaySummary
    {
        get
        {
            if (Measurements.Count == 0)
                return "No measurements added";

            var topMeasurement = Measurements.FirstOrDefault(m => m.Area == "Top");
            var sidesMeasurement = Measurements.FirstOrDefault(m => m.Area == "Sides");

            var parts = new List<string>();
            if (topMeasurement != null && !string.IsNullOrEmpty(topMeasurement.GuardSize))
                parts.Add($"Top: {topMeasurement.GuardSize}");
            if (sidesMeasurement != null && !string.IsNullOrEmpty(sidesMeasurement.GuardSize))
                parts.Add($"Sides: {sidesMeasurement.GuardSize}");

            return parts.Count > 0 ? string.Join(" | ", parts) : $"{Measurements.Count} measurement(s)";
        }
    }
}
