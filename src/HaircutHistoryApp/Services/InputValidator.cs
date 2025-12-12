using System.Text.RegularExpressions;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Provides input validation for user-entered data.
/// All methods return (bool isValid, string? errorMessage).
/// </summary>
public static partial class InputValidator
{
    // Email regex pattern - RFC 5322 simplified
    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    // Display name: letters, numbers, spaces, basic punctuation
    [GeneratedRegex(@"^[\p{L}\p{N}\s\-'.]{1,50}$", RegexOptions.Compiled)]
    private static partial Regex DisplayNameRegex();

    /// <summary>
    /// Validates an email address.
    /// </summary>
    public static (bool IsValid, string? Error) ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required");

        email = email.Trim();

        if (email.Length > 254)
            return (false, "Email address is too long");

        if (!EmailRegex().IsMatch(email))
            return (false, "Please enter a valid email address");

        return (true, null);
    }

    /// <summary>
    /// Validates a password for strength requirements.
    /// </summary>
    public static (bool IsValid, string? Error) ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return (false, "Password is required");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters");

        if (password.Length > 128)
            return (false, "Password is too long");

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);

        if (!hasUpper || !hasLower || !hasDigit)
            return (false, "Password must contain uppercase, lowercase, and a number");

        // Check for common weak passwords
        var lowerPassword = password.ToLowerInvariant();
        string[] weakPasswords = { "password", "12345678", "qwerty", "letmein", "welcome" };
        if (weakPasswords.Any(weak => lowerPassword.Contains(weak)))
            return (false, "Password is too common. Please choose a stronger password");

        return (true, null);
    }

    /// <summary>
    /// Validates a password for login (less strict than registration).
    /// </summary>
    public static (bool IsValid, string? Error) ValidateLoginPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return (false, "Password is required");

        if (password.Length < 6)
            return (false, "Password must be at least 6 characters");

        return (true, null);
    }

    /// <summary>
    /// Validates that passwords match.
    /// </summary>
    public static (bool IsValid, string? Error) ValidatePasswordsMatch(string? password, string? confirmPassword)
    {
        if (password != confirmPassword)
            return (false, "Passwords do not match");

        return (true, null);
    }

    /// <summary>
    /// Validates a display name.
    /// </summary>
    public static (bool IsValid, string? Error) ValidateDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return (false, "Display name is required");

        displayName = displayName.Trim();

        if (displayName.Length < 2)
            return (false, "Display name must be at least 2 characters");

        if (displayName.Length > 50)
            return (false, "Display name must be 50 characters or less");

        if (!DisplayNameRegex().IsMatch(displayName))
            return (false, "Display name contains invalid characters");

        return (true, null);
    }

    /// <summary>
    /// Validates a shop/salon name.
    /// </summary>
    public static (bool IsValid, string? Error) ValidateShopName(string? shopName, bool isRequired = false)
    {
        if (string.IsNullOrWhiteSpace(shopName))
        {
            return isRequired ? (false, "Shop name is required") : (true, null);
        }

        shopName = shopName.Trim();

        if (shopName.Length > 100)
            return (false, "Shop name must be 100 characters or less");

        return (true, null);
    }

    /// <summary>
    /// Validates a barber note.
    /// </summary>
    public static (bool IsValid, string? Error) ValidateBarberNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return (false, "Note cannot be empty");

        note = note.Trim();

        if (note.Length > 500)
            return (false, "Note must be 500 characters or less");

        return (true, null);
    }

    /// <summary>
    /// Validates a haircut profile name.
    /// </summary>
    public static (bool IsValid, string? Error) ValidateProfileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Profile name is required");

        name = name.Trim();

        if (name.Length < 1)
            return (false, "Profile name is required");

        if (name.Length > 50)
            return (false, "Profile name must be 50 characters or less");

        return (true, null);
    }

    /// <summary>
    /// Sanitizes user input by trimming and limiting length.
    /// </summary>
    public static string Sanitize(string? input, int maxLength = 500)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sanitized = input.Trim();

        if (sanitized.Length > maxLength)
            sanitized = sanitized.Substring(0, maxLength);

        return sanitized;
    }
}
