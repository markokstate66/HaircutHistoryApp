using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

[QueryProperty(nameof(ProfileId), "profileId")]
public partial class ProfileDetailViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private string _profileId = string.Empty;

    [ObservableProperty]
    private Profile? _profile;

    [ObservableProperty]
    private ObservableCollection<HaircutRecord> _recentHaircuts = new();

    [ObservableProperty]
    private HaircutRecord? _latestHaircut;

    [ObservableProperty]
    private ObservableCollection<HaircutMeasurement> _measurements = new();

    [ObservableProperty]
    private bool _hasLatestHaircut;

    [ObservableProperty]
    private bool _hasMeasurements;

    [ObservableProperty]
    private bool _hasRecentHaircuts;

    // Stats
    [ObservableProperty]
    private int? _daysSinceLastHaircut;

    [ObservableProperty]
    private decimal _totalSpent;

    [ObservableProperty]
    private decimal _averageCost;

    [ObservableProperty]
    private int _totalHaircuts;

    [ObservableProperty]
    private bool _hasStats;

    public string DaysSinceDisplay => DaysSinceLastHaircut.HasValue
        ? DaysSinceLastHaircut.Value == 0 ? "Today"
        : DaysSinceLastHaircut.Value == 1 ? "1 day ago"
        : $"{DaysSinceLastHaircut.Value} days ago"
        : "No haircuts";

    public ProfileDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    partial void OnProfileIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            LoadProfileCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        await ExecuteAsync(async () =>
        {
            Profile = await _dataService.GetProfileAsync(ProfileId);

            if (Profile == null)
            {
                await Shell.Current.DisplayAlertAsync("Error", "Profile not found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            Title = Profile.Name;

            // Load recent haircuts
            var haircuts = await _dataService.GetHaircutRecordsAsync(ProfileId);

            RecentHaircuts.Clear();
            // Skip the first one (latest) and take next 3 for "Recent" section
            foreach (var haircut in haircuts.Skip(1).Take(3))
            {
                RecentHaircuts.Add(haircut);
            }

            HasRecentHaircuts = RecentHaircuts.Count > 0;

            // Get latest haircut
            LatestHaircut = haircuts.FirstOrDefault();
            HasLatestHaircut = LatestHaircut != null;

            // Get latest haircut's measurements
            Measurements.Clear();

            if (LatestHaircut != null)
            {
                foreach (var measurement in LatestHaircut.Measurements)
                {
                    Measurements.Add(measurement);
                }
            }

            HasMeasurements = Measurements.Count > 0;

            // Calculate stats
            CalculateStats(haircuts);
        });
    }

    private void CalculateStats(List<HaircutRecord> haircuts)
    {
        TotalHaircuts = haircuts.Count;

        if (haircuts.Count == 0)
        {
            DaysSinceLastHaircut = null;
            TotalSpent = 0;
            AverageCost = 0;
            HasStats = false;
            return;
        }

        HasStats = true;

        // Days since last haircut
        var latestDate = haircuts.Max(h => h.Date);
        DaysSinceLastHaircut = (int)(DateTime.Today - latestDate.Date).TotalDays;

        // Total spent
        TotalSpent = haircuts.Where(h => h.Price.HasValue).Sum(h => h.Price!.Value);

        // Average cost (only haircuts with prices)
        var haircutsWithPrice = haircuts.Where(h => h.Price.HasValue).ToList();
        AverageCost = haircutsWithPrice.Count > 0
            ? haircutsWithPrice.Average(h => h.Price!.Value)
            : 0;

        // Notify DaysSinceDisplay changed
        OnPropertyChanged(nameof(DaysSinceDisplay));
    }

    [RelayCommand]
    private async Task EditProfileAsync()
    {
        if (Profile == null)
            return;

        await Shell.Current.GoToAsync($"editProfile?profileId={Profile.Id}");
    }

    [RelayCommand]
    private async Task ShareProfileAsync()
    {
        if (Profile == null)
            return;

        await Shell.Current.GoToAsync($"qrShare?profileId={Profile.Id}");
    }

    [RelayCommand]
    private async Task AddHaircutAsync()
    {
        if (Profile == null)
            return;

        await Shell.Current.GoToAsync($"addHaircut?profileId={Profile.Id}&profileName={Uri.EscapeDataString(Profile.Name)}");
    }

    [RelayCommand]
    private async Task ViewHaircutAsync(HaircutRecord haircut)
    {
        if (haircut == null || Profile == null)
            return;

        await Shell.Current.GoToAsync($"editHaircut?profileId={Profile.Id}&profileName={Uri.EscapeDataString(Profile.Name)}&recordId={haircut.Id}");
    }

    [RelayCommand]
    private async Task ViewLatestHaircutAsync()
    {
        if (LatestHaircut == null || Profile == null)
            return;

        await Shell.Current.GoToAsync($"editHaircut?profileId={Profile.Id}&profileName={Uri.EscapeDataString(Profile.Name)}&recordId={LatestHaircut.Id}");
    }

    [RelayCommand]
    private async Task ViewHaircutsAsync()
    {
        if (Profile == null)
            return;

        await Shell.Current.GoToAsync($"haircutList?profileId={Profile.Id}&profileName={Uri.EscapeDataString(Profile.Name)}");
    }

    [RelayCommand]
    private async Task StartCuttingGuideAsync()
    {
        if (Profile == null || LatestHaircut == null)
        {
            await Shell.Current.DisplayAlertAsync("No Haircuts", "Add a haircut record first to use the cutting guide.", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"cuttingGuide?profileId={Profile.Id}");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
