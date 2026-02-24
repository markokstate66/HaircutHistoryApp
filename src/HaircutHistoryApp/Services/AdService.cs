using HaircutHistoryApp.Models;
#if ANDROID || IOS
using Plugin.MauiMTAdmob;
#endif

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing AdMob advertisements.
/// Handles banner ads via MTAdView control and interstitial ads programmatically.
/// </summary>
public class AdService : IAdService, IDisposable
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogService _logService;
    private bool _isInitialized;
    private bool _disposed;
    private bool _isInterstitialLoaded;
    private DateTime _lastInterstitialShown = DateTime.MinValue;

    public bool IsInitialized => _isInitialized;
    public bool ShouldShowAds => !_subscriptionService.IsPremium;
    public bool IsInterstitialLoaded => _isInterstitialLoaded && ShouldShowAds;

    public event EventHandler? InterstitialAdClosed;

    public AdService(ISubscriptionService subscriptionService, ILogService logService)
    {
        _subscriptionService = subscriptionService;
        _logService = logService;
        _subscriptionService.SubscriptionChanged += OnSubscriptionChanged;
    }

    public Task InitializeAsync()
    {
        if (_isInitialized)
            return Task.CompletedTask;

#if ANDROID || IOS
        try
        {
            // Subscribe to interstitial events
            CrossMauiMTAdmob.Current.OnInterstitialLoaded += OnInterstitialLoaded;
            CrossMauiMTAdmob.Current.OnInterstitialFailedToLoad += OnInterstitialFailedToLoad;
            CrossMauiMTAdmob.Current.OnInterstitialClosed += OnInterstitialClosed;

            _logService.Info("AdService initialized successfully");
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to initialize AdService", exception: ex);
        }
#endif

        _isInitialized = true;
        return Task.CompletedTask;
    }

    public void LoadBannerAd()
    {
        // With Plugin.MauiMTAdmob, banner ads load automatically via MTAdView in XAML
        _logService.Info("Banner ad load requested");
    }

    public void HideBannerAd()
    {
        // Banner visibility is controlled via XAML binding to ShowAds
    }

    public void ShowBannerAd()
    {
        // Banner visibility is controlled via XAML binding to ShowAds
    }

    public void LoadInterstitialAd()
    {
        if (!ShouldShowAds)
        {
            _logService.Info("Skipping interstitial load - user is premium");
            return;
        }

#if ANDROID || IOS
        try
        {
            var adUnitId = SubscriptionConfig.GetInterstitialAdUnitId(useTestAds: IsDebugMode());
            CrossMauiMTAdmob.Current.LoadInterstitial(adUnitId);
            _logService.Info($"Loading interstitial ad: {adUnitId}");
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to load interstitial ad", exception: ex);
        }
#endif
    }

    public async Task<bool> TryShowInterstitialAdAsync()
    {
        if (!ShouldShowAds)
        {
            _logService.Info("Skipping interstitial - user is premium");
            return false;
        }

        // Check if enough time has passed since last interstitial
        var timeSinceLastAd = DateTime.UtcNow - _lastInterstitialShown;
        if (timeSinceLastAd.TotalSeconds < SubscriptionConfig.InterstitialAdIntervalSeconds)
        {
            _logService.Info($"Skipping interstitial - only {timeSinceLastAd.TotalSeconds:F0}s since last ad (interval: {SubscriptionConfig.InterstitialAdIntervalSeconds}s)");
            return false;
        }

        return await ShowInterstitialAdAsync();
    }

    public async Task<bool> ShowInterstitialAdAsync()
    {
        if (!ShouldShowAds)
        {
            return false;
        }

#if ANDROID || IOS
        try
        {
            if (CrossMauiMTAdmob.Current.IsInterstitialLoaded())
            {
                _logService.Info("Showing interstitial ad");
                CrossMauiMTAdmob.Current.ShowInterstitial();
                _lastInterstitialShown = DateTime.UtcNow;
                return true;
            }
            else
            {
                _logService.Info("Interstitial not loaded, loading now for next time");
                LoadInterstitialAd();
                return false;
            }
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to show interstitial ad", exception: ex);
            return false;
        }
#else
        await Task.CompletedTask;
        return false;
#endif
    }

#if ANDROID || IOS
    private void OnInterstitialLoaded(object? sender, EventArgs e)
    {
        _isInterstitialLoaded = true;
        _logService.Info("Interstitial ad loaded successfully");
    }

    private void OnInterstitialFailedToLoad(object? sender, EventArgs e)
    {
        _isInterstitialLoaded = false;
        _logService.Warning("Interstitial ad failed to load");
    }

    private void OnInterstitialClosed(object? sender, EventArgs e)
    {
        _isInterstitialLoaded = false;
        _logService.Info("Interstitial ad closed");

        // Fire event for any listeners
        InterstitialAdClosed?.Invoke(this, EventArgs.Empty);

        // Preload next interstitial
        LoadInterstitialAd();
    }
#endif

    private void OnSubscriptionChanged(object? sender, SubscriptionChangedEventArgs e)
    {
        if (e.NewTier == SubscriptionTier.Premium)
        {
            _logService.Info("User upgraded to premium - ads disabled");
        }
        else
        {
            _logService.Info("User is on free tier - ads enabled");
            // Preload interstitial for free users
            LoadInterstitialAd();
        }
    }

    private static bool IsDebugMode()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
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
            _subscriptionService.SubscriptionChanged -= OnSubscriptionChanged;

#if ANDROID || IOS
            try
            {
                CrossMauiMTAdmob.Current.OnInterstitialLoaded -= OnInterstitialLoaded;
                CrossMauiMTAdmob.Current.OnInterstitialFailedToLoad -= OnInterstitialFailedToLoad;
                CrossMauiMTAdmob.Current.OnInterstitialClosed -= OnInterstitialClosed;
            }
            catch
            {
                // Ignore disposal errors
            }
#endif
        }

        _disposed = true;
    }
}
