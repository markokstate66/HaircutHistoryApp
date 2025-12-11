using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class BarberDashboardViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IDataService _dataService;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private ObservableCollection<RecentClient> _recentClients = new();

    [ObservableProperty]
    private bool _hasRecentClients;

    [ObservableProperty]
    private bool _isRefreshing;

    public BarberDashboardViewModel(IAuthService authService, IDataService dataService)
    {
        _authService = authService;
        _dataService = dataService;
        Title = "Barber Dashboard";
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        await ExecuteAsync(async () =>
        {
            CurrentUser = await _authService.GetCurrentUserAsync();

            if (CurrentUser == null)
            {
                await Shell.Current.GoToAsync("//login");
                return;
            }

            var clients = await _dataService.GetRecentClientsAsync(CurrentUser.Id);

            RecentClients.Clear();
            foreach (var client in clients)
            {
                RecentClients.Add(client);
            }

            HasRecentClients = RecentClients.Count > 0;
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadDashboardAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task ScanQRCodeAsync()
    {
        await Shell.Current.GoToAsync("qrScan");
    }

    [RelayCommand]
    private async Task EnterCodeManuallyAsync()
    {
        var code = await Shell.Current.DisplayPromptAsync(
            "Enter Code",
            "Enter the client's share code:",
            maxLength: 8,
            keyboard: Keyboard.Plain);

        if (!string.IsNullOrWhiteSpace(code))
        {
            await Shell.Current.GoToAsync($"clientView?sessionId={code.Trim().ToUpperInvariant()}");
        }
    }

    [RelayCommand]
    private async Task ViewRecentClientAsync(RecentClient client)
    {
        if (client == null)
            return;

        await Shell.Current.GoToAsync($"clientView?sessionId={client.SessionId}");
    }
}
