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
        await RouteToDashboard();
    }

    private async Task RouteToDashboard()
    {
        var user = await _authService.GetCurrentUserAsync();

        if (user == null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        // Route to the profiles page (main dashboard)
        await Shell.Current.GoToAsync("//profiles");
    }
}
