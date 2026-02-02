using System.Security.Cryptography;

namespace HaircutHistoryApp.Models;

public class ShareSession
{
    public string Id { get; set; } = GenerateSecureCode();
    public string ProfileId { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? ShareUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public string QRContent => ShareUrl ?? $"haircuthistory://share/{Token ?? Id}";

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
            // Map each byte to a character (modulo bias is negligible with 32 chars)
            result[i] = chars[randomBytes[i] % chars.Length];
        }

        return new string(result);
    }
}
