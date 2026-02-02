using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class ThemeSelectionViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;
    private readonly ISubscriptionService _subscriptionService;

    [ObservableProperty]
    private ObservableCollection<ThemeDisplayModel> _themes = new();

    [ObservableProperty]
    private string _selectedThemeKey = string.Empty;

    [ObservableProperty]
    private bool _isPremium;

    public ThemeSelectionViewModel(IThemeService themeService, ISubscriptionService subscriptionService)
    {
        _themeService = themeService;
        _subscriptionService = subscriptionService;
        Title = "Themes";
    }

    [RelayCommand]
    private void LoadThemes()
    {
        IsPremium = _subscriptionService.IsPremium;
        SelectedThemeKey = _themeService.CurrentThemeKey;

        Themes.Clear();
        foreach (var theme in _themeService.GetAllThemes())
        {
            Themes.Add(new ThemeDisplayModel
            {
                Key = theme.Key,
                DisplayName = theme.DisplayName,
                Description = theme.Description,
                RequiresPremium = theme.RequiresPremium,
                IsSelected = theme.Key == SelectedThemeKey,
                IsLocked = theme.RequiresPremium && !IsPremium,
                PreviewPrimary = theme.PrimaryColor,
                PreviewSecondary = theme.SecondaryColor,
                PreviewAccent = theme.AccentColor,
                PreviewBackground = theme.BackgroundColor
            });
        }
    }

    [RelayCommand]
    private async Task SelectThemeAsync(ThemeDisplayModel theme)
    {
        if (theme == null)
            return;

        if (theme.IsLocked)
        {
            var upgrade = await Shell.Current.DisplayAlertAsync(
                "Premium Theme",
                $"The {theme.DisplayName} theme requires a Premium subscription. Upgrade now to unlock all themes!",
                "Upgrade", "Not Now");

            if (upgrade)
            {
                await Shell.Current.GoToAsync("premium");
            }
            return;
        }

        var success = await _themeService.SetThemeAsync(theme.Key);
        if (success)
        {
            SelectedThemeKey = theme.Key;

            // Update selection state
            foreach (var t in Themes)
            {
                t.IsSelected = t.Key == theme.Key;
            }

            // Force UI refresh
            var items = Themes.ToList();
            Themes.Clear();
            foreach (var item in items)
            {
                Themes.Add(item);
            }
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
