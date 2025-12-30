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

        var sessionId = _qrService.ParseQRContent(qrContent);
        if (string.IsNullOrEmpty(sessionId))
        {
            await Shell.Current.DisplayAlertAsync("Invalid Code", "The scanned code is not valid.", "OK");
            ResetScanner();
            return;
        }

        await NavigateToClientViewAsync(sessionId);
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

        var sessionId = _qrService.ParseQRContent(ManualCode.Trim().ToUpperInvariant());
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = ManualCode.Trim().ToUpperInvariant();
        }

        await NavigateToClientViewAsync(sessionId);
    }

    private async Task NavigateToClientViewAsync(string sessionId)
    {
        await ExecuteAsync(async () =>
        {
            var (profile, session) = await _dataService.GetSharedProfileAsync(sessionId);

            if (profile == null || session == null)
            {
                await Shell.Current.DisplayAlertAsync("Not Found",
                    "This share code is invalid or has expired.", "OK");
                ResetScanner();
                return;
            }

            await Shell.Current.GoToAsync($"clientView?sessionId={sessionId}");
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
