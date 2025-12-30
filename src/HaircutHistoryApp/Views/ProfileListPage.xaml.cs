using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.ViewModels;
#if ANDROID || IOS
using Plugin.MauiMTAdmob.Controls;
#endif

namespace HaircutHistoryApp.Views;

public partial class ProfileListPage : ContentPage
{
    private readonly ProfileListViewModel _viewModel;

    public ProfileListPage(ProfileListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        InitializeAdBanner();
    }

    private void InitializeAdBanner()
    {
#if ANDROID || IOS
        var adView = new MTAdView
        {
            AdsId = _viewModel.BannerAdUnitId
        };
        adView.SetBinding(IsVisibleProperty, new Binding(nameof(_viewModel.ShowAds)));
        AdBannerContainer.Content = adView;
#endif
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
