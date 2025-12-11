using System.Diagnostics;

namespace HaircutHistoryApp.Services;

public class ThemeService : IThemeService
{
    private const string ThemePreferenceKey = "app_theme";

    public AppTheme CurrentTheme { get; private set; }

    public event EventHandler<AppTheme>? ThemeChanged;

    public ThemeService()
    {
        CurrentTheme = LoadSavedTheme();
        ApplyTheme(CurrentTheme);
    }

    public void SetTheme(AppTheme theme)
    {
        if (CurrentTheme == theme)
            return;

        CurrentTheme = theme;
        SaveTheme(theme);
        ApplyTheme(theme);
        ThemeChanged?.Invoke(this, theme);

        Debug.WriteLine($"[ThemeService] Theme changed to: {theme}");
    }

    public AppTheme LoadSavedTheme()
    {
        try
        {
            var savedTheme = Preferences.Get(ThemePreferenceKey, nameof(AppTheme.System));
            if (Enum.TryParse<AppTheme>(savedTheme, out var theme))
            {
                return theme;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ThemeService] Error loading theme: {ex.Message}");
        }

        return AppTheme.System;
    }

    private void SaveTheme(AppTheme theme)
    {
        try
        {
            Preferences.Set(ThemePreferenceKey, theme.ToString());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ThemeService] Error saving theme: {ex.Message}");
        }
    }

    private void ApplyTheme(AppTheme theme)
    {
        if (Application.Current == null)
            return;

        Application.Current.UserAppTheme = theme switch
        {
            AppTheme.Light => Microsoft.Maui.ApplicationModel.AppTheme.Light,
            AppTheme.Dark => Microsoft.Maui.ApplicationModel.AppTheme.Dark,
            _ => Microsoft.Maui.ApplicationModel.AppTheme.Unspecified
        };
    }
}
