namespace HaircutHistoryApp.Services;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

/// <summary>
/// Service for structured logging throughout the application.
/// </summary>
public interface ILogService
{
    void Debug(string message, string? tag = null);
    void Info(string message, string? tag = null);
    void Warning(string message, string? tag = null, Exception? exception = null);
    void Error(string message, string? tag = null, Exception? exception = null);

    /// <summary>
    /// Log with additional context data.
    /// </summary>
    void Log(LogLevel level, string message, string? tag = null, Exception? exception = null, Dictionary<string, object>? data = null);
}
