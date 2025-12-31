using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class PremiumViewModel : BaseViewModel
{
    private readonly ISubscriptionService _subscriptionService;

    [ObservableProperty]
    private ObservableCollection<ProductInfo> _products = new();

    [ObservableProperty]
    private ProductInfo? _selectedProduct;

    [ObservableProperty]
    private bool _isPremium;

    [ObservableProperty]
    private bool _isRestoring;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private DateTime? _expirationDate;

    [ObservableProperty]
    private bool _isClientMode = true;

    [ObservableProperty]
    private bool _isBarberMode;

    [ObservableProperty]
    private bool _isMonthlySelected = true;

    [ObservableProperty]
    private bool _isYearlySelected;

    [ObservableProperty]
    private ProductInfo? _monthlyProduct;

    [ObservableProperty]
    private ProductInfo? _yearlyProduct;

    public List<string> PremiumFeatures { get; } = new()
    {
        "Up to 5 haircut profiles",
        "Attach reference photos to profiles",
        "No advertisements",
        "Cloud backup of all data",
        "Priority support"
    };

    public PremiumViewModel(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
        Title = "Go Premium";
    }

    [RelayCommand]
    private void SelectUserType(string userType)
    {
        IsClientMode = userType == "Client";
        IsBarberMode = userType == "Barber";
    }

    [RelayCommand]
    private void SelectPlan(string plan)
    {
        IsMonthlySelected = plan == "Monthly";
        IsYearlySelected = plan == "Yearly";
        SelectedProduct = IsMonthlySelected ? MonthlyProduct : YearlyProduct;
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsLoading = true;

        try
        {
            IsPremium = _subscriptionService.IsPremium;
            ExpirationDate = _subscriptionService.CurrentSubscription?.ExpirationDate;

            if (IsPremium)
                return;

            var products = await _subscriptionService.GetAvailableProductsAsync();

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            // Add placeholder products for non-mobile platforms (Windows)
            if (Products.Count == 0)
            {
                Products.Add(new ProductInfo
                {
                    ProductId = "com.haircuthistory.premium.monthly",
                    Name = "Monthly Premium",
                    Description = "Billed monthly",
                    Price = "$2.99/month"
                });
                Products.Add(new ProductInfo
                {
                    ProductId = "com.haircuthistory.premium.yearly",
                    Name = "Yearly Premium",
                    Description = "Save 50% - Best value!",
                    Price = "$17.99/year"
                });
            }

            // Set monthly and yearly products
            MonthlyProduct = Products.FirstOrDefault(p =>
                p.ProductId.Contains("monthly", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("Monthly", StringComparison.OrdinalIgnoreCase));

            YearlyProduct = Products.FirstOrDefault(p =>
                p.ProductId.Contains("yearly", StringComparison.OrdinalIgnoreCase) ||
                p.ProductId.Contains("annual", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("Year", StringComparison.OrdinalIgnoreCase));

            // Fallback if not found
            MonthlyProduct ??= Products.FirstOrDefault();
            YearlyProduct ??= Products.Skip(1).FirstOrDefault() ?? MonthlyProduct;

            // Default to monthly selected
            IsMonthlySelected = true;
            IsYearlySelected = false;
            SelectedProduct = MonthlyProduct;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectProductAsync(ProductInfo product)
    {
        SelectedProduct = product;
    }

    [RelayCommand]
    private async Task PurchaseAsync()
    {
        if (SelectedProduct == null)
        {
            await Shell.Current.DisplayAlertAsync("Select a Plan", "Please select a subscription plan to continue.", "OK");
            return;
        }

#if WINDOWS
        await Shell.Current.DisplayAlertAsync(
            "Purchase on Mobile",
            "In-app purchases are only available on the iOS and Android versions of the app. Please download the app on your mobile device to subscribe.",
            "OK");
        return;
#endif

        await ExecuteAsync(async () =>
        {
            var result = await _subscriptionService.PurchasePremiumAsync(SelectedProduct.ProductId);

            if (result.Success)
            {
                IsPremium = true;
                await Shell.Current.DisplayAlertAsync(
                    "Welcome to Premium!",
                    "You now have access to all premium features. Thank you for your support!",
                    "Awesome!");
                await Shell.Current.GoToAsync("..");
            }
            else if (result.State != PurchaseState.Cancelled)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Purchase Failed",
                    result.Error ?? "Unable to complete the purchase. Please try again.",
                    "OK");
            }
        }, "Processing purchase...");
    }

    [RelayCommand]
    private async Task RestorePurchasesAsync()
    {
        IsRestoring = true;

        try
        {
            var result = await _subscriptionService.RestorePurchasesAsync();

            if (result.Success)
            {
                IsPremium = true;
                ExpirationDate = _subscriptionService.CurrentSubscription?.ExpirationDate;
                await Shell.Current.DisplayAlertAsync(
                    "Purchases Restored",
                    "Your Premium subscription has been restored successfully!",
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "No Purchases Found",
                    "We couldn't find any previous purchases to restore. " +
                    "If you believe this is an error, please contact support.",
                    "OK");
            }
        }
        finally
        {
            IsRestoring = false;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
