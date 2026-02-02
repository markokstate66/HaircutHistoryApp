using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

/// <summary>
/// ViewModel for creating or editing a Profile (person).
/// Simplified to only handle name and avatar.
/// Measurements are now on HaircutRecord, not Profile.
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
    private string? _avatarUrl;

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
            AvatarUrl = profile.AvatarUrl;
        });
    }

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
            profile.AvatarUrl = AvatarUrl;

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
