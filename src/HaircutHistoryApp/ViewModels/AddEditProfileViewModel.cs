using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

/// <summary>
/// ViewModel for creating or editing a Profile (haircut template).
/// Profiles contain name, description, and measurements (cutting steps).
/// </summary>
[QueryProperty(nameof(ProfileId), "profileId")]
public partial class AddEditProfileViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IImageService _imageService;

    private bool _isEditMode;

    [ObservableProperty]
    private string _profileId = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string? _avatarUrl;

    [ObservableProperty]
    private string? _imageUrl1;

    [ObservableProperty]
    private string? _imageUrl2;

    [ObservableProperty]
    private string? _imageUrl3;

    [ObservableProperty]
    private ObservableCollection<HaircutMeasurement> _measurements = new();

    [ObservableProperty]
    private bool _hasMeasurements;

    public bool HasImage1 => !string.IsNullOrEmpty(ImageUrl1);
    public bool HasImage2 => !string.IsNullOrEmpty(ImageUrl2);
    public bool HasImage3 => !string.IsNullOrEmpty(ImageUrl3);

    // Picker options
    public List<string> AreaOptions => HaircutMeasurement.CommonAreas;
    public List<string> GuardSizeOptions => HaircutMeasurement.CommonGuardSizes;
    public List<string> TechniqueOptions => HaircutMeasurement.CommonTechniques;

    public AddEditProfileViewModel(
        IDataService dataService,
        IImageService imageService)
    {
        _dataService = dataService;
        _imageService = imageService;
        Title = "New Profile";
    }

    /// <summary>
    /// Gets the initials to show in the avatar when no photo is set.
    /// </summary>
    public string AvatarInitials => string.IsNullOrEmpty(Name)
        ? "?"
        : string.Concat(Name.Split(' ').Take(2).Select(w => w.Length > 0 ? w[0].ToString() : "")).ToUpper();

    partial void OnProfileIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _isEditMode = true;
            Title = "Edit Profile";
            LoadProfileCommand.Execute(null);
        }
    }

    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(nameof(AvatarInitials));
    }

    partial void OnMeasurementsChanged(ObservableCollection<HaircutMeasurement> value)
    {
        UpdateHasMeasurements();
    }

    private void UpdateHasMeasurements()
    {
        HasMeasurements = Measurements.Count > 0;
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

            Name = profile.Name;
            Description = profile.Description ?? string.Empty;
            AvatarUrl = profile.AvatarUrl;
            ImageUrl1 = profile.ImageUrl1;
            ImageUrl2 = profile.ImageUrl2;
            ImageUrl3 = profile.ImageUrl3;
            OnPropertyChanged(nameof(HasImage1));
            OnPropertyChanged(nameof(HasImage2));
            OnPropertyChanged(nameof(HasImage3));

            Measurements.Clear();
            foreach (var m in profile.Measurements.OrderBy(m => m.StepOrder))
            {
                Measurements.Add(m);
            }
            UpdateHasMeasurements();
        });
    }

    #region Photo Commands

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        var path = await _imageService.TakePhotoAsync();
        if (!string.IsNullOrEmpty(path))
        {
            AvatarUrl = path;
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        var path = await _imageService.PickImageAsync();
        if (!string.IsNullOrEmpty(path))
        {
            AvatarUrl = path;
        }
    }

    [RelayCommand]
    private void RemovePhoto()
    {
        AvatarUrl = null;
    }

    [RelayCommand]
    private async Task PickImage1Async()
    {
        var path = await _imageService.PickImageAsync();
        if (!string.IsNullOrEmpty(path))
        {
            ImageUrl1 = path;
            OnPropertyChanged(nameof(HasImage1));
        }
    }

    [RelayCommand]
    private async Task PickImage2Async()
    {
        var path = await _imageService.PickImageAsync();
        if (!string.IsNullOrEmpty(path))
        {
            ImageUrl2 = path;
            OnPropertyChanged(nameof(HasImage2));
        }
    }

    [RelayCommand]
    private async Task PickImage3Async()
    {
        var path = await _imageService.PickImageAsync();
        if (!string.IsNullOrEmpty(path))
        {
            ImageUrl3 = path;
            OnPropertyChanged(nameof(HasImage3));
        }
    }

    [RelayCommand]
    private void RemoveImage1()
    {
        ImageUrl1 = null;
        OnPropertyChanged(nameof(HasImage1));
    }

    [RelayCommand]
    private void RemoveImage2()
    {
        ImageUrl2 = null;
        OnPropertyChanged(nameof(HasImage2));
    }

    [RelayCommand]
    private void RemoveImage3()
    {
        ImageUrl3 = null;
        OnPropertyChanged(nameof(HasImage3));
    }

    #endregion

    #region Measurement Commands

    [RelayCommand]
    private void AddMeasurement()
    {
        var newMeasurement = new HaircutMeasurement
        {
            StepOrder = Measurements.Count + 1,
            Area = AreaOptions.FirstOrDefault() ?? "Top",
            GuardSize = GuardSizeOptions.FirstOrDefault() ?? string.Empty,
            Technique = TechniqueOptions.FirstOrDefault() ?? string.Empty
        };
        Measurements.Add(newMeasurement);
        UpdateHasMeasurements();
    }

    [RelayCommand]
    private void RemoveMeasurement(HaircutMeasurement measurement)
    {
        if (measurement != null)
        {
            Measurements.Remove(measurement);
            RenumberSteps();
            UpdateHasMeasurements();
        }
    }

    [RelayCommand]
    private void MoveMeasurementUp(HaircutMeasurement measurement)
    {
        if (measurement == null) return;

        var index = Measurements.IndexOf(measurement);
        if (index > 0)
        {
            Measurements.Move(index, index - 1);
            RenumberSteps();
        }
    }

    [RelayCommand]
    private void MoveMeasurementDown(HaircutMeasurement measurement)
    {
        if (measurement == null) return;

        var index = Measurements.IndexOf(measurement);
        if (index < Measurements.Count - 1)
        {
            Measurements.Move(index, index + 1);
            RenumberSteps();
        }
    }

    private void RenumberSteps()
    {
        for (int i = 0; i < Measurements.Count; i++)
        {
            Measurements[i].StepOrder = i + 1;
        }
    }

    #endregion

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlertAsync("Validation", "Please enter a name for this profile.", "OK");
            return;
        }

        await ExecuteAsync(async () =>
        {
            Profile profile;

            if (_isEditMode)
            {
                profile = await _dataService.GetProfileAsync(ProfileId) ?? new Profile();
            }
            else
            {
                profile = new Profile();
            }

            profile.Name = Name.Trim();
            profile.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            profile.AvatarUrl = AvatarUrl;
            profile.ImageUrl1 = ImageUrl1;
            profile.ImageUrl2 = ImageUrl2;
            profile.ImageUrl3 = ImageUrl3;

            // Ensure step orders are correct before saving
            RenumberSteps();
            profile.Measurements = Measurements.ToList();

            var success = await _dataService.SaveProfileAsync(profile);

            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var errorDetail = _dataService.LastError ?? "Unknown error";
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to save profile.\n\n{errorDetail}", "OK");
            }
        });
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
