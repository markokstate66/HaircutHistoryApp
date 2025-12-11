namespace HaircutHistoryApp.Services;

public interface IAlertService
{
    Task ShowErrorAsync(string message, string? title = null);

    Task ShowErrorAsync(Exception exception, string? context = null);

    Task ShowSuccessAsync(string message, string? title = null);

    Task ShowInfoAsync(string message, string? title = null);

    Task<bool> ShowConfirmAsync(string message, string? title = null, string accept = "Yes", string cancel = "No");

    Task ShowNetworkErrorAsync();

    Task ShowAuthErrorAsync();
}
