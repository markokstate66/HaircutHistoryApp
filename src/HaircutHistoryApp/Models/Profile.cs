namespace HaircutHistoryApp.Models;

/// <summary>
/// Represents a person whose haircuts are being tracked (the user, their child, etc.).
/// This is separate from HaircutRecord which tracks individual haircut events.
/// </summary>
public class Profile
{
    public string Id { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;           // "Me", "Son", "Daughter", etc.
    public string? AvatarUrl { get; set; }
    public int HaircutCount { get; set; }                      // Populated from API
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
}
