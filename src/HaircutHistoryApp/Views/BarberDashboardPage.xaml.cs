using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class BarberDashboardPage : ContentPage
{
    private readonly BarberDashboardViewModel _viewModel;

    public BarberDashboardPage(BarberDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadDashboardCommand.Execute(null);
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync("settings");
    }
}
