using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class AchievementsViewModel : BaseViewModel
{
    private readonly IPlayFabService _playFabService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<Achievement> _achievements = new();

    [ObservableProperty]
    private int _totalAchievements;

    [ObservableProperty]
    private int _unlockedCount;

    [ObservableProperty]
    private double _overallProgress;

    [ObservableProperty]
    private bool _isBarberMode;

    public AchievementsViewModel(IPlayFabService playFabService, IAuthService authService)
    {
        _playFabService = playFabService;
        _authService = authService;
        Title = "Achievements";
    }

    [RelayCommand]
    private async Task LoadAchievementsAsync()
    {
        await ExecuteAsync(async () =>
        {
            // Check if user is in barber mode
            var user = await _authService.GetCurrentUserAsync();
            IsBarberMode = user?.Mode == UserMode.Barber;

            var allAchievements = await _playFabService.GetAchievementsAsync(IsBarberMode);

            Achievements.Clear();

            // Sort: unlocked first, then by progress (closest to completion)
            var sorted = allAchievements
                .OrderByDescending(a => a.IsUnlocked)
                .ThenByDescending(a => a.Progress)
                .ThenBy(a => a.TargetValue);

            foreach (var achievement in sorted)
            {
                Achievements.Add(achievement);
            }

            TotalAchievements = allAchievements.Count;
            UnlockedCount = allAchievements.Count(a => a.IsUnlocked);
            OverallProgress = TotalAchievements > 0 ? (double)UnlockedCount / TotalAchievements : 0;
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
