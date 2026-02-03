using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.DTOs;

/// <summary>
/// Request to create a new haircut record (log entry).
/// </summary>
public class CreateHaircutRecordRequest
{
    /// <summary>
    /// When the haircut occurred.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Name of the barber/stylist.
    /// </summary>
    [JsonPropertyName("stylistName")]
    public string? StylistName { get; set; }

    /// <summary>
    /// Salon/shop name.
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>
    /// Photo URLs.
    /// </summary>
    [JsonPropertyName("photoUrls")]
    public List<string> PhotoUrls { get; set; } = new();

    /// <summary>
    /// Additional notes about this haircut.
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Price paid.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Duration in minutes.
    /// </summary>
    [JsonPropertyName("durationMinutes")]
    public int? DurationMinutes { get; set; }
}

/// <summary>
/// Request to update an existing haircut record.
/// </summary>
public class UpdateHaircutRecordRequest
{
    /// <summary>
    /// Updated date (optional).
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    /// <summary>
    /// Updated stylist name (optional).
    /// </summary>
    [JsonPropertyName("stylistName")]
    public string? StylistName { get; set; }

    /// <summary>
    /// Updated location (optional).
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>
    /// Updated photo URLs (optional).
    /// </summary>
    [JsonPropertyName("photoUrls")]
    public List<string>? PhotoUrls { get; set; }

    /// <summary>
    /// Updated notes (optional).
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Updated price (optional).
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Updated duration (optional).
    /// </summary>
    [JsonPropertyName("durationMinutes")]
    public int? DurationMinutes { get; set; }
}
