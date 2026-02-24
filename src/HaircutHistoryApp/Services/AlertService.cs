using System.Net;

namespace HaircutHistoryApp.Services;

public class AlertService : IAlertService
{
    private readonly ILogService? _log;
    private const string Tag = "AlertService";

    public AlertService(ILogService? logService = null)
    {
        _log = logService;
    }

    public async Task ShowErrorAsync(string message, string? title = null)
    {
        await ShowAlertAsync(title ?? "Error", message, "OK");
    }

    public async Task ShowErrorAsync(Exception exception, string? context = null)
    {
        var userMessage = GetUserFriendlyMessage(exception, context);
        await ShowAlertAsync("Error", userMessage, "OK");

        // Log the full exception for debugging
        _log?.Error($"Exception shown to user: {userMessage}", Tag, exception);
    }

    public async Task ShowSuccessAsync(string message, string? title = null)
    {
        await ShowAlertAsync(title ?? "Success", message, "OK");
    }

    public async Task ShowInfoAsync(string message, string? title = null)
    {
        await ShowAlertAsync(title ?? "Info", message, "OK");
    }

    public async Task<bool> ShowConfirmAsync(string message, string? title = null, string accept = "Yes", string cancel = "No")
    {
        if (Shell.Current?.CurrentPage == null)
            return false;

        return await Shell.Current.CurrentPage.DisplayAlertAsync(
            title ?? "Confirm",
            message,
            accept,
            cancel);
    }

    public async Task ShowNetworkErrorAsync()
    {
        await ShowAlertAsync(
            "Connection Error",
            "Unable to connect to the server. Please check your internet connection and try again.",
            "OK");
    }

    public async Task ShowAuthErrorAsync()
    {
        await ShowAlertAsync(
            "Authentication Error",
            "Your session has expired. Please sign in again.",
            "OK");
    }

    private static async Task ShowAlertAsync(string title, string message, string cancel)
    {
        if (Shell.Current?.CurrentPage == null)
            return;

        await Shell.Current.CurrentPage.DisplayAlertAsync(title, message, cancel);
    }

    private static string GetUserFriendlyMessage(Exception exception, string? context)
    {
        var contextPrefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";

        return exception switch
        {
            // Network errors
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Unauthorized =>
                $"{contextPrefix}Your session has expired. Please sign in again.",

            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Forbidden =>
                $"{contextPrefix}You don't have permission to perform this action.",

            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
                $"{contextPrefix}The requested item could not be found.",

            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.InternalServerError =>
                $"{contextPrefix}The server encountered an error. Please try again later.",

            HttpRequestException =>
                $"{contextPrefix}Unable to connect to the server. Please check your internet connection.",

            // Timeout
            TaskCanceledException or OperationCanceledException =>
                $"{contextPrefix}The request timed out. Please try again.",

            // Permission errors
            UnauthorizedAccessException =>
                $"{contextPrefix}You don't have permission to access this resource.",

            // IO errors
            IOException =>
                $"{contextPrefix}An error occurred while reading or writing data. Please try again.",

            // Argument errors (usually programming bugs, but can happen from bad input)
            ArgumentException argEx =>
                $"{contextPrefix}Invalid data provided: {argEx.Message}",

            // Format errors
            FormatException =>
                $"{contextPrefix}The data format is invalid. Please check your input.",

            // Default case - try to provide a helpful message
            _ => GetDefaultErrorMessage(exception, contextPrefix)
        };
    }

    private static string GetDefaultErrorMessage(Exception exception, string contextPrefix)
    {
        // If the exception message is user-friendly (not too technical), use it
        var message = exception.Message;

        // Avoid showing stack traces or technical details
        if (message.Contains("Exception") ||
            message.Contains("at ") ||
            message.Contains("stacktrace", StringComparison.OrdinalIgnoreCase) ||
            message.Length > 200)
        {
            return $"{contextPrefix}An unexpected error occurred. Please try again.";
        }

        return $"{contextPrefix}{message}";
    }
}
