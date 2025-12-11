using CommunityToolkit.Mvvm.ComponentModel;
using HaircutHistoryApp.Services;
using System.Diagnostics;

namespace HaircutHistoryApp.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    private static IAlertService? _alertService;

    protected static IAlertService AlertService =>
        _alertService ??= IPlatformApplication.Current?.Services.GetService<IAlertService>()
                          ?? new AlertService();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _loadingMessage = "Loading...";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    public bool IsNotBusy => !IsBusy;

    protected async Task ExecuteAsync(Func<Task> operation, string? loadingMessage = null, string? errorContext = null)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = null;

            if (!string.IsNullOrEmpty(loadingMessage))
                LoadingMessage = loadingMessage;

            await operation();
        }
        catch (Exception ex)
        {
            HasError = true;
            Debug.WriteLine($"[BaseViewModel] Error in ExecuteAsync: {ex}");
            await AlertService.ShowErrorAsync(ex, errorContext);
        }
        finally
        {
            IsBusy = false;
            LoadingMessage = "Loading...";
        }
    }

    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? loadingMessage = null, string? errorContext = null)
    {
        if (IsBusy)
            return default;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = null;

            if (!string.IsNullOrEmpty(loadingMessage))
                LoadingMessage = loadingMessage;

            return await operation();
        }
        catch (Exception ex)
        {
            HasError = true;
            Debug.WriteLine($"[BaseViewModel] Error in ExecuteAsync<T>: {ex}");
            await AlertService.ShowErrorAsync(ex, errorContext);
            return default;
        }
        finally
        {
            IsBusy = false;
            LoadingMessage = "Loading...";
        }
    }

    protected void SetError(string message)
    {
        HasError = true;
        ErrorMessage = message;
    }

    protected void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }
}
