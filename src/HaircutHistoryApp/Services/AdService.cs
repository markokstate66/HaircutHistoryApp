using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing AdMob advertisements.
/// With Plugin.MauiMTAdmob, ads are handled via MTAdView control in XAML.
/// This service manages ad visibility state based on subscription status.
/// </summary>
public class AdService : IAdService, IDisposable
{
    private readonly ISubscriptionService _subscriptionService;
    private bool _isInitialized;
    private bool _disposed;

    public bool IsInitialized => _isInitialized;
    public bool ShouldShowAds => !_subscriptionService.IsPremium;

    public AdService(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
        _subscriptionService.SubscriptionChanged += OnSubscriptionChanged;
    }

    public Task InitializeAsync()
    {
        if (_isInitialized)
            return Task.CompletedTask;

        // Plugin.MauiMTAdmob handles initialization automatically
        // This method is kept for interface compatibility
        _isInitialized = true;

        return Task.CompletedTask;
    }

    public void LoadBannerAd()
    {
        // With Plugin.MauiMTAdmob, banner ads load automatically via MTAdView in XAML
        // This method is kept for interface compatibility
    }

    public void HideBannerAd()
    {
        // Banner visibility is controlled via XAML binding to ShouldShowAds/ShowAds
        // This method is kept for interface compatibility
    }

    public void ShowBannerAd()
    {
        // Banner visibility is controlled via XAML binding to ShouldShowAds/ShowAds
        // This method is kept for interface compatibility
    }

    private void OnSubscriptionChanged(object? sender, SubscriptionChangedEventArgs e)
    {
        // The UI automatically updates via binding to ShowAds property in ViewModels
        // which checks ISubscriptionService.IsPremium
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Unsubscribe from events to prevent memory leaks
            _subscriptionService.SubscriptionChanged -= OnSubscriptionChanged;
        }

        _disposed = true;
    }
}
