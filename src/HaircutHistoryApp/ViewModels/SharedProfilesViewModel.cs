using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class SharedProfilesViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<Profile> _sharedProfiles = new();

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _hasProfiles;

    public SharedProfilesViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Shared With Me";
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var profiles = await _dataService.GetSharedProfilesAsync();

            SharedProfiles.Clear();
            foreach (var profile in profiles)
            {
                SharedProfiles.Add(profile);
            }

            HasProfiles = SharedProfiles.Count > 0;
        });

        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadProfilesAsync();
    }

    [RelayCommand]
    private async Task ViewProfileAsync(Profile profile)
    {
        if (profile == null)
            return;

        // Navigate to profile detail for shared profiles
        await Shell.Current.GoToAsync($"profileDetail?profileId={profile.Id}");
    }

    [RelayCommand]
    private async Task AddHaircutAsync(Profile profile)
    {
        if (profile == null)
            return;

        // Barbers can add haircut records to shared profiles
        await Shell.Current.GoToAsync($"addHaircut?profileId={profile.Id}&profileName={Uri.EscapeDataString(profile.Name)}");
    }

    [RelayCommand]
    private async Task ScanQRAsync()
    {
        await Shell.Current.GoToAsync("qrScan");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
