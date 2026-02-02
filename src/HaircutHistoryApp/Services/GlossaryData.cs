using HaircutHistoryApp.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// Static glossary data for haircut terminology.
/// </summary>
public static class GlossaryData
{
    public static List<GlossaryItem> Areas => new()
    {
        new("Top",
            "The uppermost section of the head, from the front hairline to the crown. This area typically has the longest hair and offers the most styling versatility.",
            "glossary_area_top.png"),

        new("Sides",
            "The hair on either side of the head, from the temples down to above the ears. Often cut shorter than the top for contrast.",
            "glossary_area_sides.png"),

        new("Back",
            "The rear portion of the head, from the crown down to the neckline. Can be tapered, faded, or left natural.",
            "glossary_area_back.png"),

        new("Neckline",
            "The lowest edge of hair at the back of the neck. Can be squared, rounded, tapered, or natural.",
            "glossary_area_neckline.png"),

        new("Sideburns",
            "The strip of hair in front of the ears, extending down from the hairline. Length and shape vary by style preference.",
            "glossary_area_sideburns.png"),

        new("Bangs/Fringe",
            "Hair that falls over the forehead. Can be cut straight across, side-swept, or textured.",
            "glossary_area_bangs.png"),

        new("Crown",
            "The rounded area at the top-back of the head where hair grows in a spiral pattern (cowlick). Requires special attention during cutting.",
            "glossary_area_crown.png"),

        new("Beard",
            "Facial hair on the cheeks, jaw, and chin. Can be trimmed to various lengths and shaped to complement the haircut.",
            "glossary_area_beard.png"),

        new("Mustache",
            "Hair grown on the upper lip. Styles range from natural to precisely shaped.",
            "glossary_area_mustache.png")
    };

    public static List<GlossaryItem> GuardSizes => new()
    {
        new("0 (1/16\")",
            "The shortest clipper guard, leaving about 1.5mm of hair. Creates a very close cut, nearly to the skin. Often used for skin fades.",
            "glossary_guard_0.png"),

        new("1 (1/8\")",
            "Leaves approximately 3mm of hair. Popular for tight fades and very short sides.",
            "glossary_guard_1.png"),

        new("2 (1/4\")",
            "Leaves approximately 6mm of hair. One of the most common guard sizes for sides in classic men's cuts.",
            "glossary_guard_2.png"),

        new("3 (3/8\")",
            "Leaves approximately 10mm of hair. Good for a slightly longer, softer look on the sides.",
            "glossary_guard_3.png"),

        new("4 (1/2\")",
            "Leaves approximately 13mm of hair. A medium length that works well for crew cuts and longer fades.",
            "glossary_guard_4.png"),

        new("5 (5/8\")",
            "Leaves approximately 16mm of hair. Provides a fuller look while still being manageable.",
            "glossary_guard_5.png"),

        new("6 (3/4\")",
            "Leaves approximately 19mm of hair. Good for longer styles or when transitioning to scissor work.",
            "glossary_guard_6.png"),

        new("7 (7/8\")",
            "Leaves approximately 22mm of hair. One of the longer clipper guards available.",
            "glossary_guard_7.png"),

        new("8 (1\")",
            "Leaves approximately 25mm (1 inch) of hair. The longest standard clipper guard, often used on top for buzz cuts.",
            "glossary_guard_8.png"),

        new("Scissors",
            "Hair cut using shears/scissors only, allowing for precise length control and texturing. Essential for longer styles.",
            "glossary_scissors.png"),

        new("Razor",
            "A straight or safety razor used for very close cuts, clean edges, and creating texture. Provides the closest possible cut.",
            "glossary_razor.png"),

        new("Finger Length",
            "Hair is held between the fingers and cut to the desired length. The barber uses their fingers as a guide, typically leaving 2-4 inches depending on technique.",
            "glossary_finger_length.png"),

        new("Custom",
            "A specific length not covered by standard guards, often achieved using adjustable clippers or specialized attachments.",
            null)
    };

    public static List<GlossaryItem> Techniques => new()
    {
        new("Fade",
            "A gradual transition from very short (or skin) at the bottom to longer hair on top. Types include low, mid, high, and skin fades.",
            "glossary_tech_fade.png"),

        new("Taper",
            "A gradual decrease in hair length from top to bottom, but less dramatic than a fade. Hair doesn't go as short at the edges.",
            "glossary_tech_taper.png"),

        new("Blended",
            "Seamlessly merging different lengths together so there are no visible lines or harsh transitions.",
            "glossary_tech_blended.png"),

        new("Textured",
            "Adding movement and dimension to hair by cutting at various angles. Creates a choppy, modern look with natural movement.",
            "glossary_tech_textured.png"),

        new("Layered",
            "Cutting hair at different lengths throughout to add volume, movement, and shape. Removes bulk while maintaining length.",
            "glossary_tech_layered.png"),

        new("Point Cut",
            "Cutting into the ends of hair at an angle to remove bulk and create soft, textured ends. Reduces weight without losing length.",
            "glossary_tech_pointcut.png"),

        new("Razor Cut",
            "Using a razor blade to slice through hair, creating soft, wispy ends and removing bulk. Adds texture and movement.",
            "glossary_tech_razorcut.png"),

        new("Clipper Over Comb",
            "Using clippers over a comb to blend and cut hair. Allows for precise length control in transition areas.",
            "glossary_tech_clipperovercomb.png"),

        new("Scissors Over Comb",
            "Using scissors over a comb for precise cutting and blending. Offers more control than clippers for detailed work.",
            "glossary_tech_scissorsovercomb.png"),

        new("Undercut",
            "The sides and back are cut very short or shaved, while the top is left significantly longer. Creates strong contrast.",
            "glossary_tech_undercut.png"),

        new("Disconnected",
            "A style where different sections aren't blended together, creating intentional contrast between lengths.",
            "glossary_tech_disconnected.png"),

        new("Lined Up",
            "Creating sharp, clean edges around the hairline, temples, and sideburns using clippers or a razor.",
            "glossary_tech_linedup.png"),

        new("Natural",
            "Hair left to grow and fall naturally without aggressive shaping or defined edges.",
            "glossary_tech_natural.png")
    };

    /// <summary>
    /// Gets glossary items by category.
    /// </summary>
    public static List<GlossaryItem> GetByCategory(GlossaryCategory category)
    {
        return category switch
        {
            GlossaryCategory.Areas => Areas,
            GlossaryCategory.GuardSizes => GuardSizes,
            GlossaryCategory.Techniques => Techniques,
            _ => new List<GlossaryItem>()
        };
    }

    /// <summary>
    /// Gets a specific glossary item by name.
    /// </summary>
    public static GlossaryItem? GetByName(string name)
    {
        return Areas.FirstOrDefault(x => x.Name == name)
            ?? GuardSizes.FirstOrDefault(x => x.Name == name)
            ?? Techniques.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Gets all glossary items across all categories.
    /// </summary>
    public static List<GlossaryItem> GetAll()
    {
        var all = new List<GlossaryItem>();
        all.AddRange(Areas);
        all.AddRange(GuardSizes);
        all.AddRange(Techniques);
        return all;
    }
}
