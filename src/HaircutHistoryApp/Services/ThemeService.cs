namespace HaircutHistoryApp.Services;

public class ThemeService : IThemeService
{
    private readonly ILogService _log;
    private const string ThemePreferenceKey = "app_theme";
    private const string Tag = "ThemeService";

    public AppTheme CurrentTheme { get; private set; }

    public event EventHandler<AppTheme>? ThemeChanged;

    public ThemeService(ILogService logService)
    {
        _log = logService;
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

        _log.Info($"Theme changed to: {theme}", Tag);
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
            _log.Warning("Error loading theme preference", Tag, ex);
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
            _log.Warning("Error saving theme preference", Tag, ex);
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
