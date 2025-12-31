using System.Globalization;

namespace HaircutHistoryApp.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }
}

public class StringToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasValue = !string.IsNullOrEmpty(value as string);

        // Support invert parameter
        if (parameter is string param && param.Equals("invert", StringComparison.OrdinalIgnoreCase))
            return !hasValue;

        return hasValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IntToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = false;
        if (value is int intValue)
            result = intValue > 0;

        // Support invert parameter
        if (parameter is string param && param.Equals("invert", StringComparison.OrdinalIgnoreCase))
            return !result;

        return result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToModeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isBarber)
            return isBarber ? "Barber / Stylist" : "Client";
        return "Client";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToScanTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool showManual)
            return showManual ? "Use Camera" : "Enter Manually";
        return "Enter Manually";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NameToInitialsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string name && !string.IsNullOrEmpty(name))
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return name.Substring(0, Math.Min(2, name.Length)).ToUpper();
        }
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DateToRelativeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            var diff = DateTime.UtcNow - date;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";

            return date.ToString("MMM dd");
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class AreaToIconConverter : IValueConverter
{
    private static readonly Dictionary<string, string> AreaIcons = new()
    {
        { "Top", "T" },
        { "Sides", "S" },
        { "Back", "B" },
        { "Neckline", "N" },
        { "Sideburns", "SB" },
        { "Bangs/Fringe", "F" },
        { "Crown", "C" },
        { "Beard", "BD" },
        { "Mustache", "M" }
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string area && AreaIcons.TryGetValue(area, out var icon))
            return icon;
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Achievement Converters

public class BoolToUnlockedBorderConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUnlocked && isUnlocked)
            return Color.FromArgb("#10B981"); // Success green
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUnlocked)
            return isUnlocked ? 1.0 : 0.7;
        return 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToAchievementBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUnlocked && isUnlocked)
            return Color.FromArgb("#FEF3C7"); // Warm yellow for unlocked
        return Color.FromArgb("#F1F5F9"); // Gray100
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ProgressToWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
            return progress * 200; // Max width of progress bar
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ProgressToSmallWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
            return progress * 120; // Smaller progress bar
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

/// <summary>
/// Converts a boolean to a color based on parameter.
/// Parameter format: "TrueColor|FalseColor"
/// Colors can be resource names (Primary, Secondary, etc.) or hex values.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    private static readonly Dictionary<string, Color> ColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Primary", Color.FromArgb("#6366F1") },
        { "PrimaryLight", Color.FromArgb("#E0E7FF") },
        { "Secondary", Color.FromArgb("#EC4899") },
        { "Success", Color.FromArgb("#10B981") },
        { "Warning", Color.FromArgb("#F59E0B") },
        { "Danger", Color.FromArgb("#EF4444") },
        { "Gray100", Color.FromArgb("#F1F5F9") },
        { "Gray200", Color.FromArgb("#E2E8F0") },
        { "Gray300", Color.FromArgb("#CBD5E1") },
        { "Gray400", Color.FromArgb("#94A3B8") },
        { "Gray500", Color.FromArgb("#64748B") },
        { "Gray600", Color.FromArgb("#475569") },
        { "Gray700", Color.FromArgb("#334155") },
        { "White", Colors.White },
        { "Black", Colors.Black },
        { "Transparent", Colors.Transparent },
        { "Background", Color.FromArgb("#F8FAFC") }
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string paramStr)
            return Colors.Transparent;

        var colors = paramStr.Split('|');
        if (colors.Length != 2)
            return Colors.Transparent;

        var colorName = boolValue ? colors[0].Trim() : colors[1].Trim();

        if (ColorMap.TryGetValue(colorName, out var color))
            return color;

        // Try to parse as hex
        if (colorName.StartsWith("#"))
            return Color.FromArgb(colorName);

        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
