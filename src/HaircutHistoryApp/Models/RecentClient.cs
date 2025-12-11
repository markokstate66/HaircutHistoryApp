namespace HaircutHistoryApp.Models;

public class RecentClient
{
    public string SessionId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    public string? ProfileSummary { get; set; }
}
