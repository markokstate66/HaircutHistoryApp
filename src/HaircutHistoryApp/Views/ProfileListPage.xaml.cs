using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Services;
using HaircutHistoryApp.ViewModels;
#if ANDROID || IOS
using Plugin.MauiMTAdmob.Controls;
#endif

namespace HaircutHistoryApp.Views;

public partial class ProfileListPage : ContentPage
{
    private readonly ProfileListViewModel _viewModel;
    private readonly IAdService? _adService;
    private bool _bannerInitialized;

    public ProfileListPage(ProfileListViewModel viewModel, IAdService adService)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _adService = adService;
    }

    private void InitializeAdBanner()
    {
        if (_bannerInitialized)
            return;

#if ANDROID || IOS
        try
        {
            if (_viewModel.ShowAds)
            {
                var adView = new MTAdView
                {
                    AdsId = _viewModel.BannerAdUnitId
                };
                adView.SetBinding(IsVisibleProperty, new Binding(nameof(_viewModel.ShowAds)));
                AdBannerContainer.Content = adView;
                _bannerInitialized = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AdBanner init error: {ex.Message}");
            // Ads not available - continue without them
        }
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadProfilesCommand.Execute(null);

        // Initialize banner ad if not already done
        InitializeAdBanner();

        // Try to show interstitial ad (respects interval timer)
        TryShowInterstitialAsync();
    }

    private async void TryShowInterstitialAsync()
    {
        try
        {
            if (_adService != null && _adService.ShouldShowAds)
            {
                await _adService.TryShowInterstitialAdAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Interstitial error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync("settings");
    }
}
