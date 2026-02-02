namespace HaircutHistoryApp.Models;

/// <summary>
/// Represents a single glossary entry with name, description, and optional image.
/// </summary>
public class GlossaryItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageSource { get; set; }

    public GlossaryItem() { }

    public GlossaryItem(string name, string description, string? imageSource = null)
    {
        Name = name;
        Description = description;
        ImageSource = imageSource;
    }
}

/// <summary>
/// Category of glossary items for grouping.
/// </summary>
public enum GlossaryCategory
{
    Areas,
    GuardSizes,
    Techniques
}
