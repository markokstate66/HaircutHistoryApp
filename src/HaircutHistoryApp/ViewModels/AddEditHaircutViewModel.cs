using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

/// <summary>
/// ViewModel for adding/editing haircut records.
/// Includes full measurement editing (moved from profile).
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
    private string _description = string.Empty;

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
    private ObservableCollection<MeasurementEntry> _measurements = new();

    [ObservableProperty]
    private ObservableCollection<string> _photoUrls = new();

    [ObservableProperty]
    private ObservableCollection<string> _products = new();

    [ObservableProperty]
    private string _newProduct = string.Empty;

    [ObservableProperty]
    private bool _canAddPhotos;

    public List<string> AvailableAreas => HaircutMeasurement.CommonAreas;
    public List<string> AvailableGuardSizes => HaircutMeasurement.CommonGuardSizes;
    public List<string> AvailableTechniques => HaircutMeasurement.CommonTechniques;

    public AddEditHaircutViewModel(
        IDataService dataService,
        IImageService imageService,
        ISubscriptionService subscriptionService)
    {
        _dataService = dataService;
        _imageService = imageService;
        _subscriptionService = subscriptionService;
        Title = "Add Haircut";
        InitializeDefaultMeasurements();
        _ = CheckPhotoPermissionAsync();
    }

    private async Task CheckPhotoPermissionAsync()
    {
        CanAddPhotos = await _subscriptionService.CanAddPhotosAsync();
    }

    private void InitializeDefaultMeasurements()
    {
        Measurements.Clear();
        var defaultAreas = new[] { "Top", "Sides", "Back", "Neckline" };
        var step = 1;
        foreach (var area in defaultAreas)
        {
            Measurements.Add(new MeasurementEntry { Area = area, StepOrder = step++ });
        }
    }

    partial void OnRecordIdChanged(string value)
    {
        IsEditMode = !string.IsNullOrEmpty(value);
        Title = IsEditMode ? "Edit Haircut" : "Add Haircut";

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
                Description = record.Description;
                StylistName = record.StylistName ?? string.Empty;
                Location = record.Location ?? string.Empty;
                Notes = record.Notes ?? string.Empty;
                Price = record.Price;
                DurationMinutes = record.DurationMinutes;

                // Load measurements
                Measurements.Clear();
                var step = 1;
                foreach (var m in record.Measurements.OrderBy(m => m.StepOrder))
                {
                    Measurements.Add(new MeasurementEntry
                    {
                        Area = m.Area,
                        GuardSize = m.GuardSize,
                        Technique = m.Technique,
                        Notes = m.Notes,
                        StepOrder = step++
                    });
                }

                // If no measurements, add defaults
                if (Measurements.Count == 0)
                {
                    InitializeDefaultMeasurements();
                }

                PhotoUrls.Clear();
                foreach (var url in record.PhotoUrls)
                {
                    PhotoUrls.Add(url);
                }

                Products.Clear();
                foreach (var product in record.Products)
                {
                    Products.Add(product);
                }
            }
        });
    }

    #region Measurement Management

    [RelayCommand]
    private void AddMeasurement()
    {
        Measurements.Add(new MeasurementEntry { StepOrder = Measurements.Count + 1 });
    }

    [RelayCommand]
    private void RemoveMeasurement(MeasurementEntry measurement)
    {
        if (measurement != null)
        {
            Measurements.Remove(measurement);
            UpdateStepOrders();
        }
    }

    [RelayCommand]
    private void MoveMeasurementUp(MeasurementEntry measurement)
    {
        if (measurement == null) return;

        var index = Measurements.IndexOf(measurement);
        if (index > 0)
        {
            Measurements.Move(index, index - 1);
            UpdateStepOrders();
        }
    }

    [RelayCommand]
    private void MoveMeasurementDown(MeasurementEntry measurement)
    {
        if (measurement == null) return;

        var index = Measurements.IndexOf(measurement);
        if (index < Measurements.Count - 1)
        {
            Measurements.Move(index, index + 1);
            UpdateStepOrders();
        }
    }

    private void UpdateStepOrders()
    {
        for (int i = 0; i < Measurements.Count; i++)
        {
            Measurements[i].StepOrder = i + 1;
        }
    }

    #endregion

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

    #region Product Management

    [RelayCommand]
    private void AddProduct()
    {
        if (!string.IsNullOrWhiteSpace(NewProduct))
        {
            Products.Add(NewProduct.Trim());
            NewProduct = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveProduct(string product)
    {
        if (!string.IsNullOrEmpty(product))
        {
            Products.Remove(product);
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
                Description = Description,
                StylistName = string.IsNullOrWhiteSpace(StylistName) ? null : StylistName.Trim(),
                Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                Price = Price,
                DurationMinutes = DurationMinutes,
                Measurements = Measurements
                    .Where(m => !string.IsNullOrEmpty(m.Area) &&
                               (!string.IsNullOrEmpty(m.GuardSize) ||
                                !string.IsNullOrEmpty(m.Technique) ||
                                !string.IsNullOrEmpty(m.Notes)))
                    .Select((m, index) => new HaircutMeasurement
                    {
                        Area = m.Area,
                        GuardSize = m.GuardSize ?? string.Empty,
                        Technique = m.Technique ?? string.Empty,
                        Notes = m.Notes ?? string.Empty,
                        StepOrder = index + 1
                    }).ToList(),
                PhotoUrls = PhotoUrls.ToList(),
                Products = Products.ToList()
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

/// <summary>
/// Observable entry for editing a measurement in the UI.
/// </summary>
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

    [ObservableProperty]
    private int _stepOrder;

    /// <summary>
    /// Display text showing the step number.
    /// </summary>
    public string StepDisplay => StepOrder > 0 ? $"Step {StepOrder}" : "";
}
