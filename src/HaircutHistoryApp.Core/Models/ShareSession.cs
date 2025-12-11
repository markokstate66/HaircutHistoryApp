namespace HaircutHistoryApp.Core.Models;

public class ShareSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProfileId { get; set; } = string.Empty;
    public string ClientUserId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    public bool AllowBarberNotes { get; set; } = true;

    public string ShareCode { get; set; } = GenerateShortCode();

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public string QRContent => $"haircut://{ShareCode}";

    private static string GenerateShortCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
