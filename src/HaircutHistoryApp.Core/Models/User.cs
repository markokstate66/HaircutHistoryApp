namespace HaircutHistoryApp.Core.Models;

public enum UserMode
{
    Client,
    Barber
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserMode Mode { get; set; } = UserMode.Client;
    public string? ShopName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
