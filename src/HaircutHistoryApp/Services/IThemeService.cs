namespace HaircutHistoryApp.Services;

public enum AppTheme
{
    System,
    Light,
    Dark
}

public interface IThemeService
{
    AppTheme CurrentTheme { get; }

    void SetTheme(AppTheme theme);

    AppTheme LoadSavedTheme();

    event EventHandler<AppTheme>? ThemeChanged;
}
