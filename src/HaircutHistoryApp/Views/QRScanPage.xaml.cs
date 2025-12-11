using HaircutHistoryApp.ViewModels;
using ZXing.Net.Maui;

namespace HaircutHistoryApp.Views;

public partial class QRScanPage : ContentPage
{
    private readonly QRScanViewModel _viewModel;

    public QRScanPage(QRScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        BarcodeReader.BarcodesDetected += OnBarcodesDetected;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ResetScanner();
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results?.FirstOrDefault();
        if (first != null && !string.IsNullOrEmpty(first.Value))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _viewModel.ProcessQRCodeCommand.ExecuteAsync(first.Value);
            });
        }
    }
}
