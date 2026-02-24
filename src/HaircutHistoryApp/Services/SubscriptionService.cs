using HaircutHistoryApp.Models;
using Newtonsoft.Json;
#if ANDROID || IOS
using Plugin.InAppBilling;
#endif
using AppPurchaseState = HaircutHistoryApp.Models.PurchaseState;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Service for managing user subscriptions and in-app purchases.
/// Uses local storage for subscription data.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly ILogService _logService;
    private SubscriptionInfo? _cachedSubscription;

    public SubscriptionTier CurrentTier => _cachedSubscription?.Tier ?? SubscriptionTier.Free;
    public bool IsPremium => _cachedSubscription?.IsActive ?? false;
    public SubscriptionInfo? CurrentSubscription => _cachedSubscription;

    public event EventHandler<SubscriptionChangedEventArgs>? SubscriptionChanged;

    public SubscriptionService(ILogService logService)
    {
        _logService = logService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await GetSubscriptionInfoAsync();
#if ANDROID || IOS
            await ValidateAndSyncSubscriptionAsync();
#endif
            _logService.Info($"SubscriptionService initialized. Tier: {CurrentTier}");
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to initialize SubscriptionService", exception: ex);
        }
    }

    public Task<SubscriptionInfo> GetSubscriptionInfoAsync()
    {
        if (_cachedSubscription != null)
            return Task.FromResult(_cachedSubscription);

        try
        {
            var json = Preferences.Get(SubscriptionConfig.SubscriptionDataKey, null);

            if (!string.IsNullOrEmpty(json))
            {
                _cachedSubscription = JsonConvert.DeserializeObject<SubscriptionInfo>(json);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to get subscription info from local storage", exception: ex);
        }

        _cachedSubscription ??= new SubscriptionInfo();
        return Task.FromResult(_cachedSubscription);
    }

    public async Task<bool> CanAddProfileAsync(int currentProfileCount)
    {
        var subscription = await GetSubscriptionInfoAsync();
        if (subscription.IsActive)
            return currentProfileCount < SubscriptionConfig.PremiumProfileLimit;

        return currentProfileCount < SubscriptionConfig.FreeProfileLimit;
    }

    public async Task<bool> CanAddPhotosAsync()
    {
        var subscription = await GetSubscriptionInfoAsync();
        return subscription.IsActive;
    }

    public async Task<IEnumerable<ProductInfo>> GetAvailableProductsAsync()
    {
        var products = new List<ProductInfo>();

#if ANDROID || IOS
        try
        {
            var connected = await CrossInAppBilling.Current.ConnectAsync();
            if (!connected)
            {
                _logService.Warning("Could not connect to billing service");
                return products;
            }

            // Get subscription products (monthly/yearly)
            var subscriptionIds = new[] { SubscriptionConfig.PremiumMonthlyProductId, SubscriptionConfig.PremiumYearlyProductId };
            var subscriptionItems = await CrossInAppBilling.Current.GetProductInfoAsync(
                ItemType.Subscription,
                subscriptionIds);

            foreach (var item in subscriptionItems)
            {
                products.Add(new ProductInfo
                {
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.LocalizedPrice,
                    PriceValue = item.MicrosPrice / 1_000_000m,
                    CurrencyCode = item.CurrencyCode
                });
            }

            // Get lifetime purchase (one-time)
            var lifetimeIds = new[] { SubscriptionConfig.PremiumLifetimeProductId };
            var lifetimeItems = await CrossInAppBilling.Current.GetProductInfoAsync(
                ItemType.InAppPurchase,
                lifetimeIds);

            foreach (var item in lifetimeItems)
            {
                products.Add(new ProductInfo
                {
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.LocalizedPrice,
                    PriceValue = item.MicrosPrice / 1_000_000m,
                    CurrencyCode = item.CurrencyCode
                });
            }

            _logService.Info($"Retrieved {products.Count} products from store");
        }
        catch (Exception ex)
        {
            _logService.Error("Error getting products from store", exception: ex);
        }
        finally
        {
            await DisconnectBillingAsync();
        }
#else
        _logService.Info("In-app purchases not available on this platform");
        await Task.CompletedTask;
#endif

        return products;
    }

    public async Task<PurchaseResult> PurchasePremiumAsync(string productId)
    {
#if ANDROID || IOS
        try
        {
            var connected = await CrossInAppBilling.Current.ConnectAsync();
            if (!connected)
            {
                return new PurchaseResult
                {
                    Success = false,
                    Error = "Unable to connect to store",
                    State = AppPurchaseState.Failed
                };
            }

            _logService.Info($"Attempting to purchase: {productId}");

            // Lifetime purchases are one-time (InAppPurchase), subscriptions are recurring
            var itemType = SubscriptionConfig.IsLifetimeProduct(productId)
                ? ItemType.InAppPurchase
                : ItemType.Subscription;

            var purchase = await CrossInAppBilling.Current.PurchaseAsync(
                productId,
                itemType);

            if (purchase == null)
            {
                _logService.Info("Purchase cancelled by user");
                return new PurchaseResult
                {
                    Success = false,
                    State = AppPurchaseState.Cancelled
                };
            }

            // Create subscription info from purchase
            var subscription = new SubscriptionInfo
            {
                Tier = SubscriptionTier.Premium,
                TransactionId = purchase.Id,
                ProductId = productId,
                PurchaseDate = DateTime.UtcNow,
                PurchaseState = AppPurchaseState.Purchased,
                ExpirationDate = DateTime.UtcNow.AddDays(
                    SubscriptionConfig.GetSubscriptionDurationDays(productId))
            };

            await SaveSubscriptionAsync(subscription);

            _logService.Info($"Purchase successful: {purchase.Id}");

            return new PurchaseResult
            {
                Success = true,
                State = AppPurchaseState.Purchased,
                TransactionId = purchase.Id
            };
        }
        catch (InAppBillingPurchaseException ex)
        {
            _logService.Error($"Purchase exception: {ex.PurchaseError}", exception: ex);

            var state = ex.PurchaseError switch
            {
                PurchaseError.UserCancelled => AppPurchaseState.Cancelled,
                PurchaseError.AlreadyOwned => AppPurchaseState.Purchased,
                _ => AppPurchaseState.Failed
            };

            // If already owned, try to restore
            if (ex.PurchaseError == PurchaseError.AlreadyOwned)
            {
                return await RestorePurchasesAsync();
            }

            return new PurchaseResult
            {
                Success = false,
                Error = GetPurchaseErrorMessage(ex.PurchaseError),
                State = state
            };
        }
        catch (Exception ex)
        {
            _logService.Error("Unexpected purchase error", exception: ex);
            return new PurchaseResult
            {
                Success = false,
                Error = "An unexpected error occurred",
                State = AppPurchaseState.Failed
            };
        }
        finally
        {
            await DisconnectBillingAsync();
        }
#else
        await Task.CompletedTask;
        _logService.Warning("In-app purchases not available on this platform");
        return new PurchaseResult
        {
            Success = false,
            Error = "In-app purchases are not available on this platform",
            State = AppPurchaseState.Failed
        };
#endif
    }

    public async Task<PurchaseResult> RestorePurchasesAsync()
    {
#if ANDROID || IOS
        try
        {
            var connected = await CrossInAppBilling.Current.ConnectAsync();
            if (!connected)
            {
                return new PurchaseResult
                {
                    Success = false,
                    Error = "Unable to connect to store",
                    State = AppPurchaseState.Failed
                };
            }

            _logService.Info("Attempting to restore purchases");

            var purchases = await CrossInAppBilling.Current.GetPurchasesAsync(ItemType.Subscription);

            var validPurchase = purchases?
                .Where(p => p.State == Plugin.InAppBilling.PurchaseState.Purchased ||
                           p.State == Plugin.InAppBilling.PurchaseState.Restored)
                .OrderByDescending(p => p.TransactionDateUtc)
                .FirstOrDefault();

            if (validPurchase == null)
            {
                _logService.Info("No purchases to restore");
                return new PurchaseResult
                {
                    Success = false,
                    Error = "No purchases to restore",
                    State = AppPurchaseState.Failed
                };
            }

            var subscription = new SubscriptionInfo
            {
                Tier = SubscriptionTier.Premium,
                TransactionId = validPurchase.Id,
                ProductId = validPurchase.ProductId,
                PurchaseDate = validPurchase.TransactionDateUtc,
                PurchaseState = AppPurchaseState.Restored,
                ExpirationDate = validPurchase.TransactionDateUtc.AddDays(
                    SubscriptionConfig.GetSubscriptionDurationDays(validPurchase.ProductId))
            };

            await SaveSubscriptionAsync(subscription);

            _logService.Info($"Restored purchase: {validPurchase.Id}");

            return new PurchaseResult
            {
                Success = true,
                State = AppPurchaseState.Restored,
                TransactionId = validPurchase.Id
            };
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to restore purchases", exception: ex);
            return new PurchaseResult
            {
                Success = false,
                Error = ex.Message,
                State = AppPurchaseState.Failed
            };
        }
        finally
        {
            await DisconnectBillingAsync();
        }
#else
        await Task.CompletedTask;
        _logService.Warning("In-app purchases not available on this platform");
        return new PurchaseResult
        {
            Success = false,
            Error = "In-app purchases are not available on this platform",
            State = AppPurchaseState.Failed
        };
#endif
    }

    public async Task<bool> ValidateAndSyncSubscriptionAsync()
    {
        var oldTier = CurrentTier;

        try
        {
            // Load from local storage first
            await GetSubscriptionInfoAsync();

            // Check if subscription has expired
            if (_cachedSubscription != null && _cachedSubscription.IsExpired)
            {
                _logService.Info("Subscription has expired, reverting to free tier");
                _cachedSubscription.Tier = SubscriptionTier.Free;
                await SaveSubscriptionAsync(_cachedSubscription);
            }

#if ANDROID || IOS
            // Try to validate with store
            try
            {
                var connected = await CrossInAppBilling.Current.ConnectAsync();
                if (connected)
                {
                    var purchases = await CrossInAppBilling.Current.GetPurchasesAsync(ItemType.Subscription);

                    var activePurchase = purchases?
                        .Where(p => p.State == Plugin.InAppBilling.PurchaseState.Purchased)
                        .OrderByDescending(p => p.TransactionDateUtc)
                        .FirstOrDefault();

                    if (activePurchase != null)
                    {
                        // Verify we have this purchase recorded
                        if (_cachedSubscription?.TransactionId != activePurchase.Id)
                        {
                            _logService.Info("Found newer purchase in store, updating subscription");
                            var subscription = new SubscriptionInfo
                            {
                                Tier = SubscriptionTier.Premium,
                                TransactionId = activePurchase.Id,
                                ProductId = activePurchase.ProductId,
                                PurchaseDate = activePurchase.TransactionDateUtc,
                                PurchaseState = AppPurchaseState.Purchased,
                                ExpirationDate = activePurchase.TransactionDateUtc.AddDays(
                                    SubscriptionConfig.GetSubscriptionDurationDays(activePurchase.ProductId))
                            };
                            await SaveSubscriptionAsync(subscription);
                        }
                    }
                    else if (_cachedSubscription?.Tier == SubscriptionTier.Premium && _cachedSubscription.IsExpired)
                    {
                        // No active purchase in store and our subscription is expired
                        _cachedSubscription.Tier = SubscriptionTier.Free;
                        await SaveSubscriptionAsync(_cachedSubscription);
                    }

                    await DisconnectBillingAsync();
                }
            }
            catch (Exception ex)
            {
                // Store validation is optional, don't fail if it doesn't work
                _logService.Warning($"Could not validate with store: {ex.Message}");
            }
#endif

            // Fire event if tier changed
            if (oldTier != CurrentTier)
            {
                _logService.Info($"Subscription tier changed: {oldTier} -> {CurrentTier}");
                SubscriptionChanged?.Invoke(this, new SubscriptionChangedEventArgs
                {
                    OldTier = oldTier,
                    NewTier = CurrentTier
                });
            }

            return true;
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to validate subscription", exception: ex);
            return false;
        }
    }

    private Task SaveSubscriptionAsync(SubscriptionInfo subscription)
    {
        var oldTier = _cachedSubscription?.Tier ?? SubscriptionTier.Free;
        _cachedSubscription = subscription;

        try
        {
            var json = JsonConvert.SerializeObject(subscription);
            Preferences.Set(SubscriptionConfig.SubscriptionDataKey, json);
            _logService.Info($"Subscription saved: Tier={subscription.Tier}, Expires={subscription.ExpirationDate}");
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to save subscription to local storage", exception: ex);
        }

        // Fire event if tier changed
        if (oldTier != subscription.Tier)
        {
            SubscriptionChanged?.Invoke(this, new SubscriptionChangedEventArgs
            {
                OldTier = oldTier,
                NewTier = subscription.Tier
            });
        }

        return Task.CompletedTask;
    }

#if ANDROID || IOS
    private async Task DisconnectBillingAsync()
    {
        try
        {
            await CrossInAppBilling.Current.DisconnectAsync();
        }
        catch
        {
            // Ignore disconnect errors
        }
    }

    private static string GetPurchaseErrorMessage(PurchaseError error)
    {
        return error switch
        {
            PurchaseError.UserCancelled => "Purchase was cancelled",
            PurchaseError.BillingUnavailable => "Billing service is unavailable",
            PurchaseError.PaymentNotAllowed => "Payment is not allowed on this device",
            PurchaseError.PaymentInvalid => "Payment information is invalid",
            PurchaseError.ProductRequestFailed => "Could not retrieve product information",
            PurchaseError.AppStoreUnavailable => "App Store is unavailable",
            PurchaseError.AlreadyOwned => "You already own this subscription",
            PurchaseError.NotOwned => "You do not own this subscription",
            PurchaseError.GeneralError => "A general error occurred",
            PurchaseError.DeveloperError => "Configuration error",
            PurchaseError.ItemUnavailable => "This item is not available",
            PurchaseError.RestoreFailed => "Could not restore purchases",
            PurchaseError.ServiceDisconnected => "Billing service disconnected",
            PurchaseError.ServiceTimeout => "Billing service timed out",
            PurchaseError.ServiceUnavailable => "Billing service is unavailable",
            _ => "An unexpected error occurred"
        };
    }
#endif

#if DEBUG
    public async Task SetDebugPremiumAsync(bool isPremium)
    {
        _logService.Info($"DEBUG: Setting premium mode to {isPremium}");

        var subscription = new SubscriptionInfo
        {
            Tier = isPremium ? SubscriptionTier.Premium : SubscriptionTier.Free,
            TransactionId = isPremium ? "debug-premium" : null,
            ProductId = isPremium ? "debug.premium.lifetime" : null,
            PurchaseDate = isPremium ? DateTime.UtcNow : default,
            PurchaseState = isPremium ? AppPurchaseState.Purchased : AppPurchaseState.Failed,
            ExpirationDate = isPremium ? DateTime.UtcNow.AddYears(100) : default
        };

        await SaveSubscriptionAsync(subscription);
    }
#endif
}
