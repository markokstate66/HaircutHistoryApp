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

    [ObservableProperty]
    private ObservableCollection<HaircutProfile> _profiles = new();

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isRefreshing;

    public ProfileListViewModel(IAuthService authService, IDataService dataService, IAnalyticsService analytics)
    {
        _authService = authService;
        _dataService = dataService;
        _analytics = analytics;
        Title = "My Haircuts";

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

            var profiles = await _dataService.GetProfilesAsync(user.Id);

            Profiles.Clear();
            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
            }

            IsEmpty = Profiles.Count == 0;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
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
        await Shell.Current.GoToAsync("addProfile");
    }

    [RelayCommand]
    private async Task ViewProfileAsync(HaircutProfile profile)
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
    private async Task ShareProfileAsync(HaircutProfile profile)
    {
        if (profile == null)
            return;

        await Shell.Current.GoToAsync($"qrShare?profileId={profile.Id}");
    }

    [RelayCommand]
    private async Task DeleteProfileAsync(HaircutProfile profile)
    {
        if (profile == null)
            return;

        var confirm = await Shell.Current.DisplayAlert(
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
}
