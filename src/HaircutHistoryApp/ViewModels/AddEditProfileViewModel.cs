using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

[QueryProperty(nameof(ProfileId), "profileId")]
public partial class AddEditProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IDataService _dataService;
    private readonly IImageService _imageService;
    private readonly ISubscriptionService _subscriptionService;

    private bool _isEditMode;

    [ObservableProperty]
    private string _profileId = string.Empty;

    [ObservableProperty]
    private string _profileName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _generalNotes = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MeasurementEntry> _measurements = new();

    [ObservableProperty]
    private ObservableCollection<string> _images = new();

    [ObservableProperty]
    private bool _canAddPhotos;

    public List<string> AvailableAreas => HaircutMeasurement.CommonAreas;
    public List<string> AvailableGuardSizes => HaircutMeasurement.CommonGuardSizes;
    public List<string> AvailableTechniques => HaircutMeasurement.CommonTechniques;

    public AddEditProfileViewModel(
        IAuthService authService,
        IDataService dataService,
        IImageService imageService,
        ISubscriptionService subscriptionService)
    {
        _authService = authService;
        _dataService = dataService;
        _imageService = imageService;
        _subscriptionService = subscriptionService;
        Title = "New Haircut Profile";
        InitializeDefaultMeasurements();
        _ = CheckPhotoPermissionAsync();
    }

    private async Task CheckPhotoPermissionAsync()
    {
        CanAddPhotos = await _subscriptionService.CanAddPhotosAsync();
    }

    partial void OnProfileIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _isEditMode = true;
            Title = "Edit Profile";
            LoadProfileCommand.Execute(null);
        }
    }

    private void InitializeDefaultMeasurements()
    {
        Measurements.Clear();
        var defaultAreas = new[] { "Top", "Sides", "Back", "Neckline" };
        foreach (var area in defaultAreas)
        {
            Measurements.Add(new MeasurementEntry { Area = area });
        }
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (string.IsNullOrEmpty(ProfileId))
            return;

        await ExecuteAsync(async () =>
        {
            var profile = await _dataService.GetProfileAsync(ProfileId);
            if (profile == null)
            {
                await Shell.Current.DisplayAlertAsync("Error", "Profile not found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            ProfileName = profile.Name;
            Description = profile.Description;
            GeneralNotes = profile.GeneralNotes;

            Measurements.Clear();
            foreach (var m in profile.Measurements)
            {
                Measurements.Add(new MeasurementEntry
                {
                    Area = m.Area,
                    GuardSize = m.GuardSize,
                    Technique = m.Technique,
                    Notes = m.Notes
                });
            }

            // Ensure we have the default areas if not present
            var existingAreas = Measurements.Select(m => m.Area).ToHashSet();
            var defaultAreas = new[] { "Top", "Sides", "Back", "Neckline" };
            foreach (var area in defaultAreas)
            {
                if (!existingAreas.Contains(area))
                {
                    Measurements.Add(new MeasurementEntry { Area = area });
                }
            }

            Images.Clear();
            foreach (var img in profile.LocalImagePaths.Concat(profile.ImageUrls))
            {
                Images.Add(img);
            }
        });
    }

    [RelayCommand]
    private void AddMeasurement()
    {
        Measurements.Add(new MeasurementEntry());
    }

    [RelayCommand]
    private void RemoveMeasurement(MeasurementEntry measurement)
    {
        if (measurement != null)
        {
            Measurements.Remove(measurement);
        }
    }

    [RelayCommand]
    private async Task AddImageFromGalleryAsync()
    {
        if (!CanAddPhotos)
        {
            await ShowPremiumUpsellAsync();
            return;
        }

        var path = await _imageService.PickImageAsync();
        if (!string.IsNullOrEmpty(path))
        {
            Images.Add(path);
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        if (!CanAddPhotos)
        {
            await ShowPremiumUpsellAsync();
            return;
        }

        var path = await _imageService.TakePhotoAsync();
        if (!string.IsNullOrEmpty(path))
        {
            Images.Add(path);
        }
    }

    private async Task ShowPremiumUpsellAsync()
    {
        var upgrade = await Shell.Current.DisplayAlertAsync(
            "Premium Feature",
            "Adding photos requires a Premium subscription. " +
            "Upgrade now to attach reference photos to your haircut profiles!",
            "Upgrade", "Not Now");

        if (upgrade)
        {
            await Shell.Current.GoToAsync("premium");
        }
    }

    [RelayCommand]
    private void RemoveImage(string imagePath)
    {
        if (!string.IsNullOrEmpty(imagePath))
        {
            Images.Remove(imagePath);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ProfileName))
        {
            await Shell.Current.DisplayAlertAsync("Validation", "Please enter a profile name.", "OK");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            HaircutProfile profile;

            if (_isEditMode)
            {
                profile = await _dataService.GetProfileAsync(ProfileId) ?? new HaircutProfile();
            }
            else
            {
                profile = new HaircutProfile
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
            }

            profile.Name = ProfileName;
            profile.Description = Description;
            profile.GeneralNotes = GeneralNotes;

            profile.Measurements = Measurements
                .Where(m => !string.IsNullOrEmpty(m.Area) &&
                           (!string.IsNullOrEmpty(m.GuardSize) ||
                            !string.IsNullOrEmpty(m.Technique) ||
                            !string.IsNullOrEmpty(m.Notes)))
                .Select(m => new HaircutMeasurement
                {
                    Area = m.Area,
                    GuardSize = m.GuardSize ?? string.Empty,
                    Technique = m.Technique ?? string.Empty,
                    Notes = m.Notes ?? string.Empty
                }).ToList();

            profile.LocalImagePaths = Images.ToList();
            profile.UpdatedAt = DateTime.UtcNow;

            // Set thumbnail from first available image
            profile.ThumbnailUrl = profile.ImageUrls.FirstOrDefault()
                ?? profile.LocalImagePaths.FirstOrDefault();

            var success = await _dataService.SaveProfileAsync(profile);

            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "Failed to save profile.", "OK");
            }
        });
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

public partial class MeasurementEntry : ObservableObject
{
    [ObservableProperty]
    private string _area = string.Empty;

    [ObservableProperty]
    private string? _guardSize;

    [ObservableProperty]
    private string? _technique;

    [ObservableProperty]
    private string? _notes;
}
