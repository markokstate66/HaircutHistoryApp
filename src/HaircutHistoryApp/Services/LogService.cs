using System.Diagnostics;
using System.Text;
using HaircutHistoryApp.Services.Analytics;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Structured logging service with support for multiple output targets.
/// In DEBUG mode, logs to Debug output.
/// In RELEASE mode, logs can be forwarded to crash reporting services.
/// </summary>
public class LogService : ILogService
{
    private readonly IAnalyticsService? _analyticsService;
    private readonly object _lock = new();
    private readonly string _logFilePath;
    private const int MaxLogFileSize = 1024 * 1024; // 1 MB

    public LogService(IAnalyticsService? analyticsService = null)
    {
        _analyticsService = analyticsService;
        _logFilePath = Path.Combine(FileSystem.AppDataDirectory, "app.log");
    }

    public void Debug(string message, string? tag = null)
    {
        Log(LogLevel.Debug, message, tag);
    }

    public void Info(string message, string? tag = null)
    {
        Log(LogLevel.Info, message, tag);
    }

    public void Warning(string message, string? tag = null, Exception? exception = null)
    {
        Log(LogLevel.Warning, message, tag, exception);
    }

    public void Error(string message, string? tag = null, Exception? exception = null)
    {
        Log(LogLevel.Error, message, tag, exception);
    }

    public void Log(LogLevel level, string message, string? tag = null, Exception? exception = null, Dictionary<string, object>? data = null)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelStr = level.ToString().ToUpper().PadRight(7);
        var tagStr = string.IsNullOrEmpty(tag) ? "" : $"[{tag}] ";
        var logMessage = $"{timestamp} {levelStr} {tagStr}{message}";

        // Add exception details if present
        if (exception != null)
        {
            logMessage += $"\n  Exception: {exception.GetType().Name}: {exception.Message}";
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                // Only include first 5 lines of stack trace for readability
                var stackLines = exception.StackTrace.Split('\n').Take(5);
                logMessage += $"\n  Stack: {string.Join("\n         ", stackLines)}";
            }
        }

        // Add context data if present
        if (data != null && data.Count > 0)
        {
            var dataStr = string.Join(", ", data.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            logMessage += $"\n  Data: {dataStr}";
        }

#if DEBUG
        // In debug mode, always write to Debug output
        System.Diagnostics.Debug.WriteLine(logMessage);
#endif

        // Write to log file for persistence (for errors and warnings)
        if (level >= LogLevel.Warning)
        {
            WriteToFile(logMessage);
        }

        // Track errors in analytics
        if (level == LogLevel.Error && _analyticsService != null)
        {
            var eventData = new Dictionary<string, object>
            {
                { "message", message },
                { "tag", tag ?? "none" }
            };

            if (exception != null)
            {
                eventData["exception_type"] = exception.GetType().Name;
                eventData["exception_message"] = exception.Message;
            }

            if (data != null)
            {
                foreach (var kvp in data)
                {
                    eventData[$"ctx_{kvp.Key}"] = kvp.Value;
                }
            }

            _analyticsService.TrackEvent("app_error", eventData);
        }
    }

    private void WriteToFile(string message)
    {
        try
        {
            lock (_lock)
            {
                // Rotate log if too large
                if (File.Exists(_logFilePath))
                {
                    var fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.Length > MaxLogFileSize)
                    {
                        var backupPath = _logFilePath + ".old";
                        if (File.Exists(backupPath))
                            File.Delete(backupPath);
                        File.Move(_logFilePath, backupPath);
                    }
                }

                File.AppendAllText(_logFilePath, message + Environment.NewLine);
            }
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Gets recent log entries for debugging.
    /// </summary>
    public async Task<string> GetRecentLogsAsync(int maxLines = 100)
    {
        try
        {
            if (!File.Exists(_logFilePath))
                return "No logs available.";

            var lines = await File.ReadAllLinesAsync(_logFilePath);
            var recentLines = lines.TakeLast(maxLines);
            return string.Join(Environment.NewLine, recentLines);
        }
        catch (Exception ex)
        {
            return $"Error reading logs: {ex.Message}";
        }
    }

    /// <summary>
    /// Clears old log files.
    /// </summary>
    public void ClearLogs()
    {
        try
        {
            lock (_lock)
            {
                if (File.Exists(_logFilePath))
                    File.Delete(_logFilePath);

                var backupPath = _logFilePath + ".old";
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
        }
        catch
        {
            // Clearing logs should never throw
        }
    }
}
