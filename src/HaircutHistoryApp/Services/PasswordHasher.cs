using System.Security.Cryptography;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Provides secure password hashing using PBKDF2 with SHA-256.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum

    /// <summary>
    /// Hashes a password using PBKDF2-SHA256 with a random salt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A string containing the salt and hash, separated by a delimiter.</returns>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        // Generate a random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password with the salt
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        // Combine salt and hash for storage
        // Format: iterations.salt.hash (all base64 encoded)
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hashedPassword">The stored hash to verify against.</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        try
        {
            // Parse the stored hash
            var parts = hashedPassword.Split('.');
            if (parts.Length != 3)
            {
                // Legacy format (Base64 encoded password) - migrate on next login
                return VerifyLegacyPassword(password, hashedPassword);
            }

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHash = Convert.FromBase64String(parts[2]);

            // Hash the input password with the same parameters
            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                storedHash.Length);

            // Compare using constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a stored hash needs to be upgraded (e.g., more iterations).
    /// </summary>
    /// <param name="hashedPassword">The stored hash to check.</param>
    /// <returns>True if the hash should be regenerated with current parameters.</returns>
    public static bool NeedsRehash(string hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            return true;

        var parts = hashedPassword.Split('.');
        if (parts.Length != 3)
            return true; // Legacy format

        if (!int.TryParse(parts[0], out int iterations))
            return true;

        // Rehash if using fewer iterations than current standard
        return iterations < Iterations;
    }

    /// <summary>
    /// Verifies passwords stored in the old Base64 format.
    /// This allows existing users to log in and migrate to the new format.
    /// </summary>
    private static bool VerifyLegacyPassword(string password, string storedValue)
    {
        try
        {
            // Old format was just Base64(UTF8(password))
            var legacyHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            return storedValue == legacyHash;
        }
        catch
        {
            return false;
        }
    }
}
