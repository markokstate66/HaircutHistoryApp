using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.Models;

/// <summary>
/// Represents a single haircut entry within a profile.
/// </summary>
public class HaircutRecord
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The ID of the profile this haircut belongs to.
    /// </summary>
    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who created this record (owner or stylist).
    /// </summary>
    [JsonPropertyName("createdByUserId")]
    public string CreatedByUserId { get; set; } = string.Empty;

    /// <summary>
    /// When the haircut occurred.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Description of what was done (e.g., "Fade with lineup").
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Name of the barber/stylist who performed the haircut.
    /// </summary>
    [JsonPropertyName("stylistName")]
    public string? StylistName { get; set; }

    /// <summary>
    /// Salon/shop name where the haircut was done.
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>
    /// Detailed measurements for different areas of the haircut.
    /// </summary>
    [JsonPropertyName("measurements")]
    public List<Measurement> Measurements { get; set; } = new();

    /// <summary>
    /// URLs to photos of the haircut (premium feature).
    /// </summary>
    [JsonPropertyName("photoUrls")]
    public List<string> PhotoUrls { get; set; } = new();

    /// <summary>
    /// Products used during or recommended after the haircut.
    /// </summary>
    [JsonPropertyName("products")]
    public List<string> Products { get; set; } = new();

    /// <summary>
    /// Additional notes about the haircut.
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Price paid for the haircut.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Duration of the haircut in minutes.
    /// </summary>
    [JsonPropertyName("durationMinutes")]
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Whether this record has been soft deleted.
    /// </summary>
    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the record was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the record was last updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Helper to get a display summary of the haircut.
    /// </summary>
    [JsonIgnore]
    public string DisplaySummary
    {
        get
        {
            if (!string.IsNullOrEmpty(Description))
                return Description;

            var topMeasurement = Measurements.FirstOrDefault(m => m.Area == "Top");
            var sidesMeasurement = Measurements.FirstOrDefault(m => m.Area == "Sides");

            if (topMeasurement != null || sidesMeasurement != null)
            {
                var parts = new List<string>();
                if (topMeasurement != null) parts.Add(topMeasurement.DisplayText);
                if (sidesMeasurement != null) parts.Add(sidesMeasurement.DisplayText);
                return string.Join(", ", parts);
            }

            return $"Haircut on {Date:MMM d, yyyy}";
        }
    }
}
