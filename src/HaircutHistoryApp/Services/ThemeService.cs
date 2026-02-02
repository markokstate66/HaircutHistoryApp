using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing app themes with 16 predefined themes (1 free + 15 premium).
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ISubscriptionService _subscriptionService;
    private const string ThemePreferenceKey = "selected_theme";
    private const string DefaultThemeKey = "classic_shop";

    private readonly List<ThemeDefinition> _themes;
    private ThemeDefinition _currentTheme;

    public string CurrentThemeKey => _currentTheme.Key;

    public ThemeService(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
        _themes = CreateThemeDefinitions();
        _currentTheme = _themes.First(t => t.Key == DefaultThemeKey);
    }

    public List<ThemeDefinition> GetAllThemes() => _themes;

    public ThemeDefinition GetCurrentTheme() => _currentTheme;

    public async Task InitializeAsync()
    {
        var savedThemeKey = Preferences.Get(ThemePreferenceKey, DefaultThemeKey);
        var theme = _themes.FirstOrDefault(t => t.Key == savedThemeKey) ?? _themes.First(t => t.Key == DefaultThemeKey);

        // If saved theme requires premium but user is no longer premium, reset to default
        if (theme.RequiresPremium && !_subscriptionService.IsPremium)
        {
            theme = _themes.First(t => t.Key == DefaultThemeKey);
            Preferences.Set(ThemePreferenceKey, DefaultThemeKey);
        }

        _currentTheme = theme;
        ApplyTheme(theme);
    }

    public async Task<bool> SetThemeAsync(string themeKey)
    {
        var theme = _themes.FirstOrDefault(t => t.Key == themeKey);
        if (theme == null)
            return false;

        // Check premium requirement
        if (theme.RequiresPremium && !_subscriptionService.IsPremium)
            return false;

        _currentTheme = theme;
        Preferences.Set(ThemePreferenceKey, themeKey);
        ApplyTheme(theme);
        return true;
    }

    private void ApplyTheme(ThemeDefinition theme)
    {
        var resources = Application.Current?.Resources;
        if (resources == null)
            return;

        // Main colors
        resources["Primary"] = theme.PrimaryColor;
        resources["PrimaryDark"] = theme.PrimaryColor;
        resources["PrimaryLight"] = theme.SecondaryColor;
        resources["Secondary"] = theme.AccentColor;
        resources["Tertiary"] = theme.AccentColor;

        // Semantic colors
        resources["Success"] = theme.SuccessColor;
        resources["Danger"] = theme.DestructiveColor;

        // Background colors - Light theme
        resources["BackgroundLight"] = theme.BackgroundColor;
        resources["SurfaceLight"] = theme.SurfaceColor;
        resources["SurfaceVariantLight"] = theme.SecondaryColor;

        // Dark themes that use their own dark background colors
        var isDarkTheme = theme.Key is "midnight" or "cosmic_fade" or "electric_neon";

        // Background colors - Dark theme
        resources["BackgroundDark"] = isDarkTheme ? theme.BackgroundColor : Color.FromArgb("#0F172A");
        resources["SurfaceDark"] = isDarkTheme ? theme.SurfaceColor : Color.FromArgb("#1E293B");
        resources["SurfaceVariantDark"] = isDarkTheme ? theme.SecondaryColor : Color.FromArgb("#334155");

        // Text colors
        resources["TextPrimaryLight"] = theme.TextPrimaryColor;
        resources["TextSecondaryLight"] = theme.TextSecondaryColor;
        resources["TextPrimaryDark"] = isDarkTheme ? theme.TextPrimaryColor : Color.FromArgb("#F1F5F9");
        resources["TextSecondaryDark"] = isDarkTheme ? theme.TextSecondaryColor : Color.FromArgb("#94A3B8");

        // Legacy text colors
        resources["TextPrimary"] = theme.TextPrimaryColor;
        resources["TextSecondary"] = theme.TextSecondaryColor;

        // Gray scale adjustments
        resources["Gray100"] = theme.SecondaryColor;
        resources["Gray200"] = theme.CardBorderColor;
        resources["Gray300"] = theme.DividerColor;

        // Update Shell colors
        if (Shell.Current != null)
        {
            Shell.SetBackgroundColor(Shell.Current, theme.NavigationBarColor);
            Shell.SetForegroundColor(Shell.Current, theme.NavigationBarTextColor);
            Shell.SetTitleColor(Shell.Current, theme.NavigationBarTextColor);
        }
    }

    private List<ThemeDefinition> CreateThemeDefinitions()
    {
        return new List<ThemeDefinition>
        {
            // 1. Classic Shop (Default - Free)
            new ThemeDefinition
            {
                Key = "classic_shop",
                DisplayName = "Classic Shop",
                Description = "Warm barber shop aesthetic",
                RequiresPremium = false,
                PrimaryColor = Color.FromArgb("#8B2252"),
                SecondaryColor = Color.FromArgb("#F5F0E8"),
                AccentColor = Color.FromArgb("#C8A96E"),
                TextPrimaryColor = Color.FromArgb("#2C1810"),
                TextSecondaryColor = Color.FromArgb("#6B5E55"),
                BackgroundColor = Color.FromArgb("#FAF8F5"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#E8E0D5"),
                SuccessColor = Color.FromArgb("#4A7C59"),
                DestructiveColor = Color.FromArgb("#C0392B"),
                NavigationBarColor = Color.FromArgb("#2C1810"),
                NavigationBarTextColor = Color.FromArgb("#F5F0E8"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#8B2252"),
                TabBarInactiveColor = Color.FromArgb("#6B5E55"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#8B2252"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#F5F0E8"),
                ButtonSecondaryTextColor = Color.FromArgb("#2C1810"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#E8E0D5"),
                InputFocusBorderColor = Color.FromArgb("#8B2252"),
                DividerColor = Color.FromArgb("#E8E0D5"),
                OverlayColor = Color.FromArgb("#802C1810")
            },

            // 2. Modern Fade (Premium)
            new ThemeDefinition
            {
                Key = "modern_fade",
                DisplayName = "Modern Fade",
                Description = "Clean, minimal, contemporary",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#2D3748"),
                SecondaryColor = Color.FromArgb("#EDF2F7"),
                AccentColor = Color.FromArgb("#3182CE"),
                TextPrimaryColor = Color.FromArgb("#1A202C"),
                TextSecondaryColor = Color.FromArgb("#718096"),
                BackgroundColor = Color.FromArgb("#F7FAFC"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#E2E8F0"),
                SuccessColor = Color.FromArgb("#38A169"),
                DestructiveColor = Color.FromArgb("#E53E3E"),
                NavigationBarColor = Color.FromArgb("#1A202C"),
                NavigationBarTextColor = Color.FromArgb("#FFFFFF"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#3182CE"),
                TabBarInactiveColor = Color.FromArgb("#718096"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#3182CE"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#EDF2F7"),
                ButtonSecondaryTextColor = Color.FromArgb("#2D3748"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#E2E8F0"),
                InputFocusBorderColor = Color.FromArgb("#3182CE"),
                DividerColor = Color.FromArgb("#E2E8F0"),
                OverlayColor = Color.FromArgb("#801A202C")
            },

            // 3. Vintage (Premium)
            new ThemeDefinition
            {
                Key = "vintage",
                DisplayName = "Vintage",
                Description = "Old school gentleman's shop",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#5D4037"),
                SecondaryColor = Color.FromArgb("#EFEBE9"),
                AccentColor = Color.FromArgb("#BF6B3F"),
                TextPrimaryColor = Color.FromArgb("#3E2723"),
                TextSecondaryColor = Color.FromArgb("#8D6E63"),
                BackgroundColor = Color.FromArgb("#FAF6F1"),
                SurfaceColor = Color.FromArgb("#FFFDF9"),
                CardBorderColor = Color.FromArgb("#D7CCC8"),
                SuccessColor = Color.FromArgb("#558B2F"),
                DestructiveColor = Color.FromArgb("#BF360C"),
                NavigationBarColor = Color.FromArgb("#3E2723"),
                NavigationBarTextColor = Color.FromArgb("#EFEBE9"),
                TabBarColor = Color.FromArgb("#FFFDF9"),
                TabBarActiveColor = Color.FromArgb("#BF6B3F"),
                TabBarInactiveColor = Color.FromArgb("#8D6E63"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#5D4037"),
                ButtonPrimaryTextColor = Color.FromArgb("#EFEBE9"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#EFEBE9"),
                ButtonSecondaryTextColor = Color.FromArgb("#3E2723"),
                InputBackgroundColor = Color.FromArgb("#FFFDF9"),
                InputBorderColor = Color.FromArgb("#D7CCC8"),
                InputFocusBorderColor = Color.FromArgb("#BF6B3F"),
                DividerColor = Color.FromArgb("#D7CCC8"),
                OverlayColor = Color.FromArgb("#803E2723")
            },

            // 4. Midnight (Premium) - Dark theme
            new ThemeDefinition
            {
                Key = "midnight",
                DisplayName = "Midnight",
                Description = "Dark mode, upscale salon",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#7C3AED"),
                SecondaryColor = Color.FromArgb("#1E1E2E"),
                AccentColor = Color.FromArgb("#A0AEC0"),
                TextPrimaryColor = Color.FromArgb("#E2E8F0"),
                TextSecondaryColor = Color.FromArgb("#A0AEC0"),
                BackgroundColor = Color.FromArgb("#0F0F1A"),
                SurfaceColor = Color.FromArgb("#1E1E2E"),
                CardBorderColor = Color.FromArgb("#2D2D44"),
                SuccessColor = Color.FromArgb("#48BB78"),
                DestructiveColor = Color.FromArgb("#FC8181"),
                NavigationBarColor = Color.FromArgb("#0F0F1A"),
                NavigationBarTextColor = Color.FromArgb("#E2E8F0"),
                TabBarColor = Color.FromArgb("#1E1E2E"),
                TabBarActiveColor = Color.FromArgb("#7C3AED"),
                TabBarInactiveColor = Color.FromArgb("#A0AEC0"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#7C3AED"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#2D2D44"),
                ButtonSecondaryTextColor = Color.FromArgb("#E2E8F0"),
                InputBackgroundColor = Color.FromArgb("#1E1E2E"),
                InputBorderColor = Color.FromArgb("#2D2D44"),
                InputFocusBorderColor = Color.FromArgb("#7C3AED"),
                DividerColor = Color.FromArgb("#2D2D44"),
                OverlayColor = Color.FromArgb("#80000000")
            },

            // 5. Fresh Cut (Premium)
            new ThemeDefinition
            {
                Key = "fresh_cut",
                DisplayName = "Fresh Cut",
                Description = "Bright, energetic, youthful",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#276749"),
                SecondaryColor = Color.FromArgb("#F0FFF4"),
                AccentColor = Color.FromArgb("#ED8936"),
                TextPrimaryColor = Color.FromArgb("#1A3A2A"),
                TextSecondaryColor = Color.FromArgb("#68876F"),
                BackgroundColor = Color.FromArgb("#F7FDF9"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#C6F6D5"),
                SuccessColor = Color.FromArgb("#38A169"),
                DestructiveColor = Color.FromArgb("#E53E3E"),
                NavigationBarColor = Color.FromArgb("#1A3A2A"),
                NavigationBarTextColor = Color.FromArgb("#F0FFF4"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#276749"),
                TabBarInactiveColor = Color.FromArgb("#68876F"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#276749"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#F0FFF4"),
                ButtonSecondaryTextColor = Color.FromArgb("#1A3A2A"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#C6F6D5"),
                InputFocusBorderColor = Color.FromArgb("#276749"),
                DividerColor = Color.FromArgb("#C6F6D5"),
                OverlayColor = Color.FromArgb("#801A3A2A")
            },

            // 6. Pastel Studio (Premium)
            new ThemeDefinition
            {
                Key = "pastel_studio",
                DisplayName = "Pastel Studio",
                Description = "Soft, salon-forward aesthetic",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#B83280"),
                SecondaryColor = Color.FromArgb("#FFF5F7"),
                AccentColor = Color.FromArgb("#805AD5"),
                TextPrimaryColor = Color.FromArgb("#4A2040"),
                TextSecondaryColor = Color.FromArgb("#9B7A8F"),
                BackgroundColor = Color.FromArgb("#FFFAFB"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#FED7E2"),
                SuccessColor = Color.FromArgb("#68D391"),
                DestructiveColor = Color.FromArgb("#FC8181"),
                NavigationBarColor = Color.FromArgb("#4A2040"),
                NavigationBarTextColor = Color.FromArgb("#FFF5F7"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#B83280"),
                TabBarInactiveColor = Color.FromArgb("#9B7A8F"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#B83280"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#FFF5F7"),
                ButtonSecondaryTextColor = Color.FromArgb("#4A2040"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#FED7E2"),
                InputFocusBorderColor = Color.FromArgb("#B83280"),
                DividerColor = Color.FromArgb("#FED7E2"),
                OverlayColor = Color.FromArgb("#804A2040")
            },

            // ===== EXPANSION PACK THEMES =====

            // 7. Cosmic Fade (Premium) - Dark theme
            new ThemeDefinition
            {
                Key = "cosmic_fade",
                DisplayName = "Cosmic Fade",
                Description = "Deep space vibes with nebula accents",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#1B0A3C"),
                SecondaryColor = Color.FromArgb("#0D0D2B"),
                AccentColor = Color.FromArgb("#00E5FF"),
                TextPrimaryColor = Color.FromArgb("#E8E0F0"),
                TextSecondaryColor = Color.FromArgb("#9F8FBF"),
                BackgroundColor = Color.FromArgb("#0A0A1F"),
                SurfaceColor = Color.FromArgb("#151535"),
                CardBorderColor = Color.FromArgb("#2A2A5A"),
                SuccessColor = Color.FromArgb("#00E676"),
                DestructiveColor = Color.FromArgb("#FF5252"),
                NavigationBarColor = Color.FromArgb("#0A0A1F"),
                NavigationBarTextColor = Color.FromArgb("#00E5FF"),
                TabBarColor = Color.FromArgb("#151535"),
                TabBarActiveColor = Color.FromArgb("#00E5FF"),
                TabBarInactiveColor = Color.FromArgb("#9F8FBF"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#6200EA"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#2A2A5A"),
                ButtonSecondaryTextColor = Color.FromArgb("#E8E0F0"),
                InputBackgroundColor = Color.FromArgb("#151535"),
                InputBorderColor = Color.FromArgb("#2A2A5A"),
                InputFocusBorderColor = Color.FromArgb("#00E5FF"),
                DividerColor = Color.FromArgb("#2A2A5A"),
                OverlayColor = Color.FromArgb("#990A0A1F")
            },

            // 8. Jungle Chop (Premium)
            new ThemeDefinition
            {
                Key = "jungle_chop",
                DisplayName = "Jungle Chop",
                Description = "Lush tropical canopy energy",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#1B5E20"),
                SecondaryColor = Color.FromArgb("#E8F5E9"),
                AccentColor = Color.FromArgb("#FF6D00"),
                TextPrimaryColor = Color.FromArgb("#0D2E11"),
                TextSecondaryColor = Color.FromArgb("#4E7A51"),
                BackgroundColor = Color.FromArgb("#F1F8E9"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#A5D6A7"),
                SuccessColor = Color.FromArgb("#2E7D32"),
                DestructiveColor = Color.FromArgb("#D84315"),
                NavigationBarColor = Color.FromArgb("#0D2E11"),
                NavigationBarTextColor = Color.FromArgb("#E8F5E9"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#1B5E20"),
                TabBarInactiveColor = Color.FromArgb("#4E7A51"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#1B5E20"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#E8F5E9"),
                ButtonSecondaryTextColor = Color.FromArgb("#0D2E11"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#A5D6A7"),
                InputFocusBorderColor = Color.FromArgb("#FF6D00"),
                DividerColor = Color.FromArgb("#A5D6A7"),
                OverlayColor = Color.FromArgb("#800D2E11")
            },

            // 9. Electric Neon (Premium) - Dark theme
            new ThemeDefinition
            {
                Key = "electric_neon",
                DisplayName = "Electric Neon",
                Description = "Late night glow-up energy",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#FF00FF"),
                SecondaryColor = Color.FromArgb("#1A1A2E"),
                AccentColor = Color.FromArgb("#39FF14"),
                TextPrimaryColor = Color.FromArgb("#F0F0F0"),
                TextSecondaryColor = Color.FromArgb("#B0B0C8"),
                BackgroundColor = Color.FromArgb("#0F0F1E"),
                SurfaceColor = Color.FromArgb("#1A1A2E"),
                CardBorderColor = Color.FromArgb("#2E2E4A"),
                SuccessColor = Color.FromArgb("#39FF14"),
                DestructiveColor = Color.FromArgb("#FF1744"),
                NavigationBarColor = Color.FromArgb("#0F0F1E"),
                NavigationBarTextColor = Color.FromArgb("#FF00FF"),
                TabBarColor = Color.FromArgb("#1A1A2E"),
                TabBarActiveColor = Color.FromArgb("#FF00FF"),
                TabBarInactiveColor = Color.FromArgb("#B0B0C8"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#FF00FF"),
                ButtonPrimaryTextColor = Color.FromArgb("#0F0F1E"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#2E2E4A"),
                ButtonSecondaryTextColor = Color.FromArgb("#39FF14"),
                InputBackgroundColor = Color.FromArgb("#1A1A2E"),
                InputBorderColor = Color.FromArgb("#2E2E4A"),
                InputFocusBorderColor = Color.FromArgb("#39FF14"),
                DividerColor = Color.FromArgb("#2E2E4A"),
                OverlayColor = Color.FromArgb("#990F0F1E")
            },

            // 10. Aloha Trim (Premium)
            new ThemeDefinition
            {
                Key = "aloha_trim",
                DisplayName = "Aloha Trim",
                Description = "Island vibes and ocean breeze",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#00838F"),
                SecondaryColor = Color.FromArgb("#FFF8E1"),
                AccentColor = Color.FromArgb("#FF6F61"),
                TextPrimaryColor = Color.FromArgb("#1A3C40"),
                TextSecondaryColor = Color.FromArgb("#5F8A8B"),
                BackgroundColor = Color.FromArgb("#FFFDF5"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#B2DFDB"),
                SuccessColor = Color.FromArgb("#2E7D32"),
                DestructiveColor = Color.FromArgb("#D32F2F"),
                NavigationBarColor = Color.FromArgb("#1A3C40"),
                NavigationBarTextColor = Color.FromArgb("#FFF8E1"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#00838F"),
                TabBarInactiveColor = Color.FromArgb("#5F8A8B"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#00838F"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#FFF8E1"),
                ButtonSecondaryTextColor = Color.FromArgb("#1A3C40"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#B2DFDB"),
                InputFocusBorderColor = Color.FromArgb("#FF6F61"),
                DividerColor = Color.FromArgb("#B2DFDB"),
                OverlayColor = Color.FromArgb("#801A3C40")
            },

            // 11. Groovy Baby (Premium)
            new ThemeDefinition
            {
                Key = "groovy_baby",
                DisplayName = "Groovy Baby",
                Description = "60s/70s flower power and good vibes",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#E65100"),
                SecondaryColor = Color.FromArgb("#FFF3E0"),
                AccentColor = Color.FromArgb("#F9A825"),
                TextPrimaryColor = Color.FromArgb("#3E2723"),
                TextSecondaryColor = Color.FromArgb("#8D6E63"),
                BackgroundColor = Color.FromArgb("#FFFDE7"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#FFCC80"),
                SuccessColor = Color.FromArgb("#558B2F"),
                DestructiveColor = Color.FromArgb("#C62828"),
                NavigationBarColor = Color.FromArgb("#3E2723"),
                NavigationBarTextColor = Color.FromArgb("#FFF3E0"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#E65100"),
                TabBarInactiveColor = Color.FromArgb("#8D6E63"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#E65100"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#FFF3E0"),
                ButtonSecondaryTextColor = Color.FromArgb("#3E2723"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#FFCC80"),
                InputFocusBorderColor = Color.FromArgb("#F9A825"),
                DividerColor = Color.FromArgb("#FFCC80"),
                OverlayColor = Color.FromArgb("#803E2723")
            },

            // 12. Disco Cuts (Premium)
            new ThemeDefinition
            {
                Key = "disco_cuts",
                DisplayName = "Disco Cuts",
                Description = "Late 70s/early 80s dance floor glam",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#AA00FF"),
                SecondaryColor = Color.FromArgb("#F3E5F5"),
                AccentColor = Color.FromArgb("#FFD600"),
                TextPrimaryColor = Color.FromArgb("#1A0033"),
                TextSecondaryColor = Color.FromArgb("#7B5EA7"),
                BackgroundColor = Color.FromArgb("#FDF5FF"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#CE93D8"),
                SuccessColor = Color.FromArgb("#00C853"),
                DestructiveColor = Color.FromArgb("#FF1744"),
                NavigationBarColor = Color.FromArgb("#1A0033"),
                NavigationBarTextColor = Color.FromArgb("#FFD600"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#AA00FF"),
                TabBarInactiveColor = Color.FromArgb("#7B5EA7"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#AA00FF"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#F3E5F5"),
                ButtonSecondaryTextColor = Color.FromArgb("#1A0033"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#CE93D8"),
                InputFocusBorderColor = Color.FromArgb("#FFD600"),
                DividerColor = Color.FromArgb("#CE93D8"),
                OverlayColor = Color.FromArgb("#801A0033")
            },

            // 13. Radical Razor (Premium)
            new ThemeDefinition
            {
                Key = "radical_razor",
                DisplayName = "Radical Razor",
                Description = "80s neon Miami and hair metal vibes",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#E91E63"),
                SecondaryColor = Color.FromArgb("#FCE4EC"),
                AccentColor = Color.FromArgb("#00BCD4"),
                TextPrimaryColor = Color.FromArgb("#1A0A1A"),
                TextSecondaryColor = Color.FromArgb("#7A5080"),
                BackgroundColor = Color.FromArgb("#FFF0F5"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#F8BBD0"),
                SuccessColor = Color.FromArgb("#00E676"),
                DestructiveColor = Color.FromArgb("#FF3D00"),
                NavigationBarColor = Color.FromArgb("#1A0A1A"),
                NavigationBarTextColor = Color.FromArgb("#E91E63"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#E91E63"),
                TabBarInactiveColor = Color.FromArgb("#7A5080"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#E91E63"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#FCE4EC"),
                ButtonSecondaryTextColor = Color.FromArgb("#1A0A1A"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#F8BBD0"),
                InputFocusBorderColor = Color.FromArgb("#00BCD4"),
                DividerColor = Color.FromArgb("#F8BBD0"),
                OverlayColor = Color.FromArgb("#801A0A1A")
            },

            // 14. Fresh Prince (Premium)
            new ThemeDefinition
            {
                Key = "fresh_prince",
                DisplayName = "Fresh Prince",
                Description = "90s bold color block swagger",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#1565C0"),
                SecondaryColor = Color.FromArgb("#E3F2FD"),
                AccentColor = Color.FromArgb("#FFC107"),
                TextPrimaryColor = Color.FromArgb("#0D1B2A"),
                TextSecondaryColor = Color.FromArgb("#546E8A"),
                BackgroundColor = Color.FromArgb("#F5F9FF"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#90CAF9"),
                SuccessColor = Color.FromArgb("#2E7D32"),
                DestructiveColor = Color.FromArgb("#D32F2F"),
                NavigationBarColor = Color.FromArgb("#0D1B2A"),
                NavigationBarTextColor = Color.FromArgb("#FFC107"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#1565C0"),
                TabBarInactiveColor = Color.FromArgb("#546E8A"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#1565C0"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#E3F2FD"),
                ButtonSecondaryTextColor = Color.FromArgb("#0D1B2A"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#90CAF9"),
                InputFocusBorderColor = Color.FromArgb("#FFC107"),
                DividerColor = Color.FromArgb("#90CAF9"),
                OverlayColor = Color.FromArgb("#800D1B2A")
            },

            // 15. Salon Rosé (Premium)
            new ThemeDefinition
            {
                Key = "salon_rose",
                DisplayName = "Salon Rosé",
                Description = "Upscale salon luxury with a soft blush finish",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#AD1457"),
                SecondaryColor = Color.FromArgb("#FDE8EF"),
                AccentColor = Color.FromArgb("#D4AF37"),
                TextPrimaryColor = Color.FromArgb("#3C0A1E"),
                TextSecondaryColor = Color.FromArgb("#8E5068"),
                BackgroundColor = Color.FromArgb("#FFFAFC"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#F5C6D0"),
                SuccessColor = Color.FromArgb("#43A047"),
                DestructiveColor = Color.FromArgb("#C62828"),
                NavigationBarColor = Color.FromArgb("#3C0A1E"),
                NavigationBarTextColor = Color.FromArgb("#FDE8EF"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#AD1457"),
                TabBarInactiveColor = Color.FromArgb("#8E5068"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#AD1457"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#FDE8EF"),
                ButtonSecondaryTextColor = Color.FromArgb("#3C0A1E"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#F5C6D0"),
                InputFocusBorderColor = Color.FromArgb("#D4AF37"),
                DividerColor = Color.FromArgb("#F5C6D0"),
                OverlayColor = Color.FromArgb("#803C0A1E")
            },

            // 16. Platinum Lounge (Premium)
            new ThemeDefinition
            {
                Key = "platinum_lounge",
                DisplayName = "Platinum Lounge",
                Description = "Sleek, minimal, premium salon sophistication",
                RequiresPremium = true,
                PrimaryColor = Color.FromArgb("#212121"),
                SecondaryColor = Color.FromArgb("#F5F5F5"),
                AccentColor = Color.FromArgb("#B0BEC5"),
                TextPrimaryColor = Color.FromArgb("#0A0A0A"),
                TextSecondaryColor = Color.FromArgb("#757575"),
                BackgroundColor = Color.FromArgb("#FAFAFA"),
                SurfaceColor = Color.FromArgb("#FFFFFF"),
                CardBorderColor = Color.FromArgb("#E0E0E0"),
                SuccessColor = Color.FromArgb("#388E3C"),
                DestructiveColor = Color.FromArgb("#B71C1C"),
                NavigationBarColor = Color.FromArgb("#0A0A0A"),
                NavigationBarTextColor = Color.FromArgb("#B0BEC5"),
                TabBarColor = Color.FromArgb("#FFFFFF"),
                TabBarActiveColor = Color.FromArgb("#212121"),
                TabBarInactiveColor = Color.FromArgb("#757575"),
                ButtonPrimaryBackgroundColor = Color.FromArgb("#212121"),
                ButtonPrimaryTextColor = Color.FromArgb("#FFFFFF"),
                ButtonSecondaryBackgroundColor = Color.FromArgb("#F5F5F5"),
                ButtonSecondaryTextColor = Color.FromArgb("#0A0A0A"),
                InputBackgroundColor = Color.FromArgb("#FFFFFF"),
                InputBorderColor = Color.FromArgb("#E0E0E0"),
                InputFocusBorderColor = Color.FromArgb("#B0BEC5"),
                DividerColor = Color.FromArgb("#E0E0E0"),
                OverlayColor = Color.FromArgb("#800A0A0A")
            }
        };
    }
}
