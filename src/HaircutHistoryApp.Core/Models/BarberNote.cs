namespace HaircutHistoryApp.Core.Models;

public class BarberNote
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BarberId { get; set; } = string.Empty;
    public string BarberName { get; set; } = string.Empty;
    public string? ShopName { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
