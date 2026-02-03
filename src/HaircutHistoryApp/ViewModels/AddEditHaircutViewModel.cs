using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

/// <summary>
/// ViewModel for adding/editing haircut log entries.
/// This is a simple log - measurements live on the Profile.
/// </summary>
[QueryProperty(nameof(ProfileId), "profileId")]
[QueryProperty(nameof(ProfileName), "profileName")]
[QueryProperty(nameof(RecordId), "recordId")]
public partial class AddEditHaircutViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IImageService _imageService;
    private readonly ISubscriptionService _subscriptionService;

    [ObservableProperty]
    private string _profileId = string.Empty;

    [ObservableProperty]
    private string _profileName = string.Empty;

    [ObservableProperty]
    private string _recordId = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private string _stylistName = string.Empty;

    [ObservableProperty]
    private string _location = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private decimal? _price;

    [ObservableProperty]
    private int? _durationMinutes;

    [ObservableProperty]
    private ObservableCollection<string> _photoUrls = new();

    [ObservableProperty]
    private bool _canAddPhotos;

    public AddEditHaircutViewModel(
        IDataService dataService,
        IImageService imageService,
        ISubscriptionService subscriptionService)
    {
        _dataService = dataService;
        _imageService = imageService;
        _subscriptionService = subscriptionService;
        Title = "Log Haircut";
        _ = CheckPhotoPermissionAsync();
    }

    private async Task CheckPhotoPermissionAsync()
    {
        CanAddPhotos = await _subscriptionService.CanAddPhotosAsync();
    }

    partial void OnRecordIdChanged(string value)
    {
        IsEditMode = !string.IsNullOrEmpty(value);
        Title = IsEditMode ? "Edit Haircut Log" : "Log Haircut";

        if (IsEditMode)
        {
            LoadRecordCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadRecordAsync()
    {
        if (string.IsNullOrEmpty(ProfileId) || string.IsNullOrEmpty(RecordId))
            return;

        await ExecuteAsync(async () =>
        {
            var record = await _dataService.GetHaircutRecordAsync(ProfileId, RecordId);

            if (record != null)
            {
                Date = record.Date;
                StylistName = record.StylistName ?? string.Empty;
                Location = record.Location ?? string.Empty;
                Notes = record.Notes ?? string.Empty;
                Price = record.Price;
                DurationMinutes = record.DurationMinutes;

                PhotoUrls.Clear();
                foreach (var url in record.PhotoUrls)
                {
                    PhotoUrls.Add(url);
                }
            }
        });
    }

    #region Photo Management

    [RelayCommand]
    private async Task AddPhotoFromGalleryAsync()
    {
        if (!CanAddPhotos)
        {
            await ShowPremiumUpsellAsync();
            return;
        }

        var path = await _imageService.PickImageAsync();
        if (!string.IsNullOrEmpty(path))
        {
            PhotoUrls.Add(path);
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
            PhotoUrls.Add(path);
        }
    }

    private async Task ShowPremiumUpsellAsync()
    {
        var upgrade = await Shell.Current.DisplayAlertAsync(
            "Premium Feature",
            "Adding photos requires a Premium subscription. " +
            "Upgrade now to attach photos to your haircut records!",
            "Upgrade", "Not Now");

        if (upgrade)
        {
            await Shell.Current.GoToAsync("premium");
        }
    }

    [RelayCommand]
    private void RemovePhoto(string photoUrl)
    {
        if (!string.IsNullOrEmpty(photoUrl))
        {
            PhotoUrls.Remove(photoUrl);
        }
    }

    #endregion

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(ProfileId))
        {
            await Shell.Current.DisplayAlertAsync("Error", "Profile ID is required.", "OK");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var record = new HaircutRecord
            {
                Id = IsEditMode ? RecordId : string.Empty,
                ProfileId = ProfileId,
                Date = Date,
                StylistName = string.IsNullOrWhiteSpace(StylistName) ? null : StylistName.Trim(),
                Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                Price = Price,
                DurationMinutes = DurationMinutes,
                PhotoUrls = PhotoUrls.ToList()
            };

            var success = await _dataService.SaveHaircutRecordAsync(ProfileId, record);

            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var errorDetail = _dataService.LastError ?? "Unknown error";
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to save haircut record.\n\n{errorDetail}", "OK");
            }
        });
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
