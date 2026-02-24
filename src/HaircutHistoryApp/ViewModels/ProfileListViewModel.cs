using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;
using HaircutHistoryApp.Services.Analytics;

namespace HaircutHistoryApp.ViewModels;

public partial class ProfileListViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IDataService _dataService;
    private readonly IAnalyticsService _analytics;
    private readonly ISubscriptionService _subscriptionService;

    [ObservableProperty]
    private ObservableCollection<Profile> _profiles = new();

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _showUpgradeBanner;

    [ObservableProperty]
    private int _remainingProfiles;

    [ObservableProperty]
    private bool _showAds;

    // Global Stats
    [ObservableProperty]
    private int? _daysSinceLastHaircut;

    [ObservableProperty]
    private decimal _spentThisYear;

    [ObservableProperty]
    private decimal _spentPast90Days;

    [ObservableProperty]
    private int _totalHaircutsThisYear;

    [ObservableProperty]
    private bool _hasGlobalStats;

    public string GlobalDaysSinceDisplay => DaysSinceLastHaircut.HasValue
        ? DaysSinceLastHaircut.Value == 0 ? "Today"
        : DaysSinceLastHaircut.Value == 1 ? "1 day ago"
        : $"{DaysSinceLastHaircut.Value} days"
        : "--";

    public string BannerAdUnitId =>
#if DEBUG
        SubscriptionConfig.GetBannerAdUnitId(useTestAds: true);
#else
        SubscriptionConfig.GetBannerAdUnitId(useTestAds: false);
#endif

    public ProfileListViewModel(
        IAuthService authService,
        IDataService dataService,
        IAnalyticsService analytics,
        ISubscriptionService subscriptionService)
    {
        _authService = authService;
        _dataService = dataService;
        _analytics = analytics;
        _subscriptionService = subscriptionService;
        Title = "Haircut Profiles";

        _analytics.TrackScreen(AnalyticsScreens.ProfileList);
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var user = await _authService.GetCurrentUserAsync();

            if (user == null)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            var profiles = await _dataService.GetProfilesAsync();

            Profiles.Clear();
            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
            }

            IsEmpty = Profiles.Count == 0;

            // Calculate global stats
            await CalculateGlobalStatsAsync();

            // Update subscription status
            await UpdateSubscriptionStatusAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadProfilesAsync();
    }

    [RelayCommand]
    private async Task AddProfileAsync()
    {
        var canAdd = await _subscriptionService.CanAddProfileAsync(Profiles.Count);
        var isPremium = _subscriptionService.IsPremium;

        if (!canAdd)
        {
            if (isPremium)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Profile Limit Reached",
                    $"You've reached the maximum of {SubscriptionConfig.PremiumProfileLimit} profiles.",
                    "OK");
            }
            else
            {
                var upgrade = await Shell.Current.DisplayAlertAsync(
                    "Profile Limit Reached",
                    $"Free accounts can only have {SubscriptionConfig.FreeProfileLimit} profile. " +
                    $"Upgrade to Premium for up to {SubscriptionConfig.PremiumProfileLimit} profiles!",
                    "Upgrade", "Cancel");

                if (upgrade)
                {
                    await Shell.Current.GoToAsync("premium");
                }
            }
            return;
        }

        await Shell.Current.GoToAsync("addProfile");
    }

    private async Task CalculateGlobalStatsAsync()
    {
        try
        {
            var allHaircuts = new List<HaircutRecord>();

            // Load haircuts from all profiles
            foreach (var profile in Profiles)
            {
                var haircuts = await _dataService.GetHaircutRecordsAsync(profile.Id);
                allHaircuts.AddRange(haircuts);
            }

            if (allHaircuts.Count == 0)
            {
                HasGlobalStats = false;
                return;
            }

            HasGlobalStats = true;

            var today = DateTime.Today;
            var startOfYear = new DateTime(today.Year, 1, 1);
            var past90Days = today.AddDays(-90);

            // Days since last haircut (any profile)
            var latestDate = allHaircuts.Max(h => h.Date);
            DaysSinceLastHaircut = (int)(today - latestDate.Date).TotalDays;

            // Spent this year
            SpentThisYear = allHaircuts
                .Where(h => h.Date >= startOfYear && h.Price.HasValue)
                .Sum(h => h.Price!.Value);

            // Spent past 90 days
            SpentPast90Days = allHaircuts
                .Where(h => h.Date >= past90Days && h.Price.HasValue)
                .Sum(h => h.Price!.Value);

            // Total haircuts this year
            TotalHaircutsThisYear = allHaircuts.Count(h => h.Date >= startOfYear);

            OnPropertyChanged(nameof(GlobalDaysSinceDisplay));
        }
        catch
        {
            HasGlobalStats = false;
        }
    }

    private async Task UpdateSubscriptionStatusAsync()
    {
        var isPremium = _subscriptionService.IsPremium;
        ShowUpgradeBanner = !isPremium;
        ShowAds = !isPremium;

        if (isPremium)
            RemainingProfiles = SubscriptionConfig.PremiumProfileLimit - Profiles.Count;
        else
            RemainingProfiles = SubscriptionConfig.FreeProfileLimit - Profiles.Count;
    }

    [RelayCommand]
    private async Task GoToPremiumAsync()
    {
        await Shell.Current.GoToAsync("premium");
    }

    [RelayCommand]
    private async Task ViewProfileAsync(Profile profile)
    {
        if (profile == null)
            return;

        _analytics.TrackEvent(AnalyticsEvents.ProfileViewed, new Dictionary<string, object>
        {
            { AnalyticsProperties.ProfileId, profile.Id },
            { AnalyticsProperties.ProfileName, profile.Name }
        });

        await Shell.Current.GoToAsync($"profileDetail?profileId={profile.Id}");
    }

    [RelayCommand]
    private async Task ShareProfileAsync(Profile profile)
    {
        if (profile == null)
            return;

        await Shell.Current.GoToAsync($"qrShare?profileId={profile.Id}");
    }

    [RelayCommand]
    private async Task DeleteProfileAsync(Profile profile)
    {
        if (profile == null)
            return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Delete Profile",
            $"Are you sure you want to delete \"{profile.Name}\"?",
            "Delete", "Cancel");

        if (confirm)
        {
            await _dataService.DeleteProfileAsync(profile.Id);
            Profiles.Remove(profile);
            IsEmpty = Profiles.Count == 0;

            _analytics.TrackEvent(AnalyticsEvents.ProfileDeleted, new Dictionary<string, object>
            {
                { AnalyticsProperties.ProfileId, profile.Id },
                { AnalyticsProperties.ProfileName, profile.Name }
            });
        }
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync("settings");
    }

    [RelayCommand]
    private async Task GoToSharedProfilesAsync()
    {
        await Shell.Current.GoToAsync("sharedProfiles");
    }

    [RelayCommand]
    private async Task ScanQRAsync()
    {
        await Shell.Current.GoToAsync("qrScan");
    }
}
