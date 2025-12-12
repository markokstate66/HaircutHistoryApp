using System.Security.Cryptography;

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

    public string ShareCode { get; set; } = GenerateSecureCode();

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public string QRContent => $"haircut://{ShareCode}";

    /// <summary>
    /// Generates a cryptographically secure 8-character code.
    /// Uses characters that are easy to read and type (no 0, O, 1, I, L).
    /// Provides ~40 bits of entropy (32^8 = 1 trillion+ combinations).
    /// </summary>
    private static string GenerateSecureCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 32 characters
        const int codeLength = 8;

        // Use cryptographically secure random bytes
        var randomBytes = RandomNumberGenerator.GetBytes(codeLength);
        var result = new char[codeLength];

        for (int i = 0; i < codeLength; i++)
        {
            result[i] = chars[randomBytes[i] % chars.Length];
        }

        return new string(result);
    }
}
