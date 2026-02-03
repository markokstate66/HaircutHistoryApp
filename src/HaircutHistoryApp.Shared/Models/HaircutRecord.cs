using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.Models;

/// <summary>
/// Represents a log entry of when a haircut profile was used.
/// This is a simple record - measurements live on the Profile.
/// </summary>
public class HaircutRecord
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The ID of the profile (haircut template) this record is for.
    /// </summary>
    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who created this record.
    /// </summary>
    [JsonPropertyName("createdByUserId")]
    public string CreatedByUserId { get; set; } = string.Empty;

    /// <summary>
    /// When the haircut occurred.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

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
    /// URLs to photos of the haircut.
    /// </summary>
    [JsonPropertyName("photoUrls")]
    public List<string> PhotoUrls { get; set; } = new();

    /// <summary>
    /// Additional notes about this specific haircut.
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
    /// Display summary for list views.
    /// </summary>
    [JsonIgnore]
    public string DisplaySummary => !string.IsNullOrEmpty(Notes)
        ? Notes
        : $"Haircut on {Date:MMM d, yyyy}";
}
