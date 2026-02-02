using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class QRScanViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IQRService _qrService;

    [ObservableProperty]
    private bool _isScanning = true;

    [ObservableProperty]
    private string _manualCode = string.Empty;

    [ObservableProperty]
    private bool _showManualEntry;

    private bool _hasProcessedCode;

    public QRScanViewModel(IDataService dataService, IQRService qrService)
    {
        _dataService = dataService;
        _qrService = qrService;
        Title = "Scan QR Code";
    }

    public void ResetScanner()
    {
        _hasProcessedCode = false;
        IsScanning = true;
    }

    [RelayCommand]
    private async Task ProcessQRCodeAsync(string qrContent)
    {
        if (_hasProcessedCode || string.IsNullOrEmpty(qrContent))
            return;

        _hasProcessedCode = true;
        IsScanning = false;

        var token = _qrService.ParseQRContent(qrContent);
        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.DisplayAlertAsync("Invalid Code", "The scanned code is not valid.", "OK");
            ResetScanner();
            return;
        }

        await AcceptShareAsync(token);
    }

    [RelayCommand]
    private void ToggleManualEntry()
    {
        ShowManualEntry = !ShowManualEntry;
        if (ShowManualEntry)
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task SubmitManualCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(ManualCode))
        {
            await Shell.Current.DisplayAlertAsync("Validation", "Please enter a share code.", "OK");
            return;
        }

        var token = _qrService.ParseQRContent(ManualCode.Trim());
        if (string.IsNullOrEmpty(token))
        {
            // Treat manual input as the token itself
            token = ManualCode.Trim();
        }

        await AcceptShareAsync(token);
    }

    private async Task AcceptShareAsync(string token)
    {
        await ExecuteAsync(async () =>
        {
            var success = await _dataService.AcceptShareAsync(token);

            if (!success)
            {
                var errorDetail = _dataService.LastError ?? "This share code is invalid or has expired.";
                await Shell.Current.DisplayAlertAsync("Share Failed", errorDetail, "OK");
                ResetScanner();
                return;
            }

            // Share accepted - navigate to shared profiles
            await Shell.Current.DisplayAlertAsync("Success",
                "Profile has been shared with you! You can now view it in 'Shared With Me'.", "OK");

            // Navigate to shared profiles page
            await Shell.Current.GoToAsync("//shared");
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
