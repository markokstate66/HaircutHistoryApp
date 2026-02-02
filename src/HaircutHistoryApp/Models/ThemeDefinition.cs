namespace HaircutHistoryApp.Models;

/// <summary>
/// Represents a complete theme definition with all colors.
/// </summary>
public class ThemeDefinition
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresPremium { get; set; }

    // Main colors
    public Color PrimaryColor { get; set; } = Colors.Black;
    public Color SecondaryColor { get; set; } = Colors.White;
    public Color AccentColor { get; set; } = Colors.Blue;

    // Text colors
    public Color TextPrimaryColor { get; set; } = Colors.Black;
    public Color TextSecondaryColor { get; set; } = Colors.Gray;

    // Background colors
    public Color BackgroundColor { get; set; } = Colors.White;
    public Color SurfaceColor { get; set; } = Colors.White;
    public Color CardBorderColor { get; set; } = Colors.LightGray;

    // Semantic colors
    public Color SuccessColor { get; set; } = Colors.Green;
    public Color DestructiveColor { get; set; } = Colors.Red;

    // Navigation colors
    public Color NavigationBarColor { get; set; } = Colors.Black;
    public Color NavigationBarTextColor { get; set; } = Colors.White;
    public Color TabBarColor { get; set; } = Colors.White;
    public Color TabBarActiveColor { get; set; } = Colors.Blue;
    public Color TabBarInactiveColor { get; set; } = Colors.Gray;

    // Button colors
    public Color ButtonPrimaryBackgroundColor { get; set; } = Colors.Blue;
    public Color ButtonPrimaryTextColor { get; set; } = Colors.White;
    public Color ButtonSecondaryBackgroundColor { get; set; } = Colors.LightGray;
    public Color ButtonSecondaryTextColor { get; set; } = Colors.Black;

    // Input colors
    public Color InputBackgroundColor { get; set; } = Colors.White;
    public Color InputBorderColor { get; set; } = Colors.LightGray;
    public Color InputFocusBorderColor { get; set; } = Colors.Blue;

    // Other
    public Color DividerColor { get; set; } = Colors.LightGray;
    public Color OverlayColor { get; set; } = Color.FromArgb("#80000000");
}

/// <summary>
/// Display model for theme selection UI.
/// </summary>
public class ThemeDisplayModel
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresPremium { get; set; }
    public bool IsSelected { get; set; }
    public bool IsLocked { get; set; }
    public Color PreviewPrimary { get; set; } = Colors.Black;
    public Color PreviewSecondary { get; set; } = Colors.White;
    public Color PreviewAccent { get; set; } = Colors.Blue;
    public Color PreviewBackground { get; set; } = Colors.White;
}
