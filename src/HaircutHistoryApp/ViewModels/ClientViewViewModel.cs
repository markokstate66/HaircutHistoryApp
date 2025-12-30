using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

[QueryProperty(nameof(SessionId), "sessionId")]
public partial class ClientViewViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IDataService _dataService;

    [ObservableProperty]
    private string _sessionId = string.Empty;

    [ObservableProperty]
    private HaircutProfile? _profile;

    [ObservableProperty]
    private ShareSession? _session;

    [ObservableProperty]
    private string _clientName = string.Empty;

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

    [ObservableProperty]
    private bool _canAddNotes;

    [ObservableProperty]
    private string _newNote = string.Empty;

    [ObservableProperty]
    private bool _isAddingNote;

    public ClientViewViewModel(IAuthService authService, IDataService dataService)
    {
        _authService = authService;
        _dataService = dataService;
        Title = "Client Profile";
    }

    partial void OnSessionIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            LoadClientProfileCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadClientProfileAsync()
    {
        await ExecuteAsync(async () =>
        {
            var (profile, session) = await _dataService.GetSharedProfileAsync(SessionId);

            if (profile == null || session == null)
            {
                await Shell.Current.DisplayAlertAsync("Error",
                    "Profile not found or share link has expired.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            Profile = profile;
            Session = session;
            ClientName = session.ClientName;
            Title = $"{ClientName}'s Haircut";
            CanAddNotes = session.AllowBarberNotes;

            Measurements.Clear();
            foreach (var m in profile.Measurements)
            {
                Measurements.Add(m);
            }

            BarberNotes.Clear();
            foreach (var note in profile.BarberNotes.OrderByDescending(n => n.CreatedAt))
            {
                BarberNotes.Add(note);
            }

            Images.Clear();
            foreach (var img in profile.LocalImagePaths.Concat(profile.ImageUrls))
            {
                Images.Add(img);
            }

            HasImages = Images.Count > 0;
            HasBarberNotes = BarberNotes.Count > 0;

            // Save as recent client
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                await _dataService.SaveRecentClientAsync(currentUser.Id, new RecentClient
                {
                    SessionId = SessionId,
                    ClientName = ClientName,
                    ProfileName = profile.Name,
                    ViewedAt = DateTime.UtcNow,
                    ProfileSummary = profile.DisplaySummary
                });
            }
        });
    }

    [RelayCommand]
    private void ShowAddNote()
    {
        IsAddingNote = true;
        NewNote = string.Empty;
    }

    [RelayCommand]
    private void CancelAddNote()
    {
        IsAddingNote = false;
        NewNote = string.Empty;
    }

    [RelayCommand]
    private async Task SaveNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(NewNote))
        {
            await Shell.Current.DisplayAlertAsync("Validation", "Please enter a note.", "OK");
            return;
        }

        if (Profile == null)
            return;

        await ExecuteAsync(async () =>
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            var note = new BarberNote
            {
                BarberId = currentUser.Id,
                BarberName = currentUser.DisplayName,
                ShopName = currentUser.ShopName,
                Note = NewNote.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var success = await _dataService.AddBarberNoteAsync(Profile.Id, note);

            if (success)
            {
                BarberNotes.Insert(0, note);
                HasBarberNotes = true;
                IsAddingNote = false;
                NewNote = string.Empty;
                await Shell.Current.DisplayAlertAsync("Success", "Note added successfully!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "Failed to add note.", "OK");
            }
        });
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
