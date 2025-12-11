using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

[QueryProperty(nameof(ProfileId), "profileId")]
public partial class QRShareViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IDataService _dataService;
    private readonly IQRService _qrService;

    [ObservableProperty]
    private string _profileId = string.Empty;

    [ObservableProperty]
    private HaircutProfile? _profile;

    [ObservableProperty]
    private ShareSession? _shareSession;

    [ObservableProperty]
    private ImageSource? _qrCodeImage;

    [ObservableProperty]
    private string _shareCode = string.Empty;

    [ObservableProperty]
    private bool _allowBarberNotes = true;

    [ObservableProperty]
    private string _expiresIn = string.Empty;

    public QRShareViewModel(IAuthService authService, IDataService dataService, IQRService qrService)
    {
        _authService = authService;
        _dataService = dataService;
        _qrService = qrService;
        Title = "Share Profile";
    }

    partial void OnProfileIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            GenerateShareCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task GenerateShareAsync()
    {
        await ExecuteAsync(async () =>
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            Profile = await _dataService.GetProfileAsync(ProfileId);
            if (Profile == null)
            {
                await Shell.Current.DisplayAlert("Error", "Profile not found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            ShareSession = await _dataService.CreateShareSessionAsync(
                ProfileId, user.Id, user.DisplayName, AllowBarberNotes);

            ShareCode = ShareSession.Id;
            ExpiresIn = "24 hours";

            GenerateQRCode();
        });
    }

    partial void OnAllowBarberNotesChanged(bool value)
    {
        if (ShareSession != null)
        {
            ShareSession.AllowBarberNotes = value;
            RegenerateShareCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task RegenerateShareAsync()
    {
        if (string.IsNullOrEmpty(ProfileId))
            return;

        await GenerateShareAsync();
    }

    private void GenerateQRCode()
    {
        if (ShareSession == null)
            return;

        try
        {
            var qrBytes = _qrService.GenerateQRCode(ShareSession.QRContent, 300, 300);
            if (qrBytes != null && qrBytes.Length > 0)
            {
                QrCodeImage = ImageSource.FromStream(() => new MemoryStream(qrBytes));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"QR Generation error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CopyCodeAsync()
    {
        if (!string.IsNullOrEmpty(ShareCode))
        {
            await Clipboard.SetTextAsync(ShareCode);
            await Shell.Current.DisplayAlert("Copied", "Share code copied to clipboard!", "OK");
        }
    }

    [RelayCommand]
    private async Task ShareLinkAsync()
    {
        if (ShareSession == null)
            return;

        await Share.RequestAsync(new ShareTextRequest
        {
            Title = "Share Haircut Profile",
            Text = $"View my haircut profile with code: {ShareCode}\n\nOr scan this in the HairCut History app."
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
