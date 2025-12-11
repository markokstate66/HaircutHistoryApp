using System.Diagnostics;

namespace HaircutHistoryApp.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private string? _userId;
    private readonly Dictionary<string, string> _userProperties = new();
    private bool _isInitialized;

    public void Initialize()
    {
        if (_isInitialized) return;

        // In production, initialize your analytics SDK here
        // Examples: Firebase Analytics, App Center, Mixpanel, etc.

#if DEBUG
        Debug.WriteLine("[Analytics] Initialized");
#endif

        _isInitialized = true;
    }

    public void TrackEvent(string eventName, Dictionary<string, object>? properties = null)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

#if DEBUG
        var propsString = properties != null
            ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}"))
            : "none";
        Debug.WriteLine($"[Analytics] Event: {eventName} | Properties: {propsString}");
#endif

        // In production, send to your analytics provider:
        // FirebaseAnalytics.LogEvent(eventName, properties);
        // Analytics.TrackEvent(eventName, properties);
    }

    public void TrackScreen(string screenName)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

#if DEBUG
        Debug.WriteLine($"[Analytics] Screen: {screenName}");
#endif

        // In production:
        // FirebaseAnalytics.SetCurrentScreen(screenName);
    }

    public void SetUserId(string userId)
    {
        _userId = userId;

#if DEBUG
        Debug.WriteLine($"[Analytics] User ID: {userId}");
#endif

        // In production:
        // FirebaseAnalytics.SetUserId(userId);
    }

    public void SetUserProperty(string name, string value)
    {
        _userProperties[name] = value;

#if DEBUG
        Debug.WriteLine($"[Analytics] User Property: {name}={value}");
#endif

        // In production:
        // FirebaseAnalytics.SetUserProperty(name, value);
    }
}
