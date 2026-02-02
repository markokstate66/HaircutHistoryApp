using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing app themes.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Get all available theme definitions.
    /// </summary>
    List<ThemeDefinition> GetAllThemes();

    /// <summary>
    /// Get the currently active theme.
    /// </summary>
    ThemeDefinition GetCurrentTheme();

    /// <summary>
    /// Get the current theme key.
    /// </summary>
    string CurrentThemeKey { get; }

    /// <summary>
    /// Apply a theme. Checks premium status before applying premium themes.
    /// Returns false if user is not premium and theme requires premium.
    /// </summary>
    Task<bool> SetThemeAsync(string themeKey);

    /// <summary>
    /// Load and apply the saved theme on app startup.
    /// Falls back to classic_shop if saved theme requires premium and user is no longer premium.
    /// </summary>
    Task InitializeAsync();
}
