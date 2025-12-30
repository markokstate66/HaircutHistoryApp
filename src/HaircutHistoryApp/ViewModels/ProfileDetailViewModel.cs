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
    private HaircutProfile? _profile;

    [ObservableProperty]
    private ObservableCollection<HaircutMeasurement> _measurements = new();

    [ObservableProperty]
    private ObservableCollection<BarberNote> _barberNotes = new();

    [ObservableProperty]
    private ObservableCollection<string> _images = new();

    [ObservableProperty]
    private bool _hasImages;

    [ObservableProperty]
    private bool _hasBarberNotes;

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

            Measurements.Clear();
            foreach (var measurement in Profile.Measurements)
            {
                Measurements.Add(measurement);
            }

            BarberNotes.Clear();
            foreach (var note in Profile.BarberNotes.OrderByDescending(n => n.CreatedAt))
            {
                BarberNotes.Add(note);
            }

            Images.Clear();
            foreach (var image in Profile.LocalImagePaths.Concat(Profile.ImageUrls))
            {
                Images.Add(image);
            }

            HasImages = Images.Count > 0;
            HasBarberNotes = BarberNotes.Count > 0;
        });
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
    private async Task ViewImageAsync(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return;

        await Shell.Current.GoToAsync($"imageViewer?imagePath={Uri.EscapeDataString(imagePath)}");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
