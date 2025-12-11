using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class ProfileListPage : ContentPage
{
    private readonly ProfileListViewModel _viewModel;

    public ProfileListPage(ProfileListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadProfilesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync("settings");
    }
}
