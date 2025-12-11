using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.Views;

public partial class MainPage : ContentPage
{
    private readonly IAuthService _authService;

    public MainPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RouteToCorrectDashboard();
    }

    private async Task RouteToCorrectDashboard()
    {
        var user = await _authService.GetCurrentUserAsync();

        if (user == null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        if (user.Mode == UserMode.Barber)
        {
            await Shell.Current.GoToAsync("//barberDashboard");
        }
        else
        {
            await Shell.Current.GoToAsync("//clientDashboard");
        }
    }
}
