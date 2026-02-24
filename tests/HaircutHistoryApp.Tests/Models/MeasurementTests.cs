using FluentAssertions;
using HaircutHistoryApp.Shared.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.Models;

public class MeasurementTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var measurement = new Measurement();

        measurement.Area.Should().BeEmpty();
        measurement.GuardSize.Should().BeEmpty();
        measurement.Technique.Should().BeEmpty();
        measurement.Notes.Should().BeEmpty();
        measurement.StepOrder.Should().Be(0);
    }

    [Fact]
    public void DisplayText_WithAllFields_FormatsCorrectly()
    {
        var measurement = new Measurement
        {
            Area = "Top",
            GuardSize = "4",
            Technique = "Scissors"
        };

        measurement.DisplayText.Should().Be("Top: #4 (Scissors)");
    }

    [Fact]
    public void DisplayText_WithNoGuardSize_OmitsGuardNumber()
    {
        var measurement = new Measurement
        {
            Area = "Top",
            GuardSize = "",
            Technique = "Scissors"
        };

        measurement.DisplayText.Should().Be("Top:  (Scissors)");
    }

    [Fact]
    public void DisplayText_WithNoTechnique_OmitsTechniqueParens()
    {
        var measurement = new Measurement
        {
            Area = "Sides",
            GuardSize = "2",
            Technique = ""
        };

        measurement.DisplayText.Should().Be("Sides: #2");
    }

    [Fact]
    public void DisplayText_WithNeitherGuardNorTechnique_ShowsAreaOnly()
    {
        var measurement = new Measurement
        {
            Area = "Back",
            GuardSize = "",
            Technique = ""
        };

        measurement.DisplayText.Should().Be("Back: ");
    }

    [Fact]
    public void CommonAreas_ContainsExpectedValues()
    {
        Measurement.CommonAreas.Should().Contain(new[]
        {
            "Top", "Sides", "Back", "Neckline", "Sideburns",
            "Bangs/Fringe", "Crown", "Beard", "Mustache"
        });
    }

    [Fact]
    public void CommonGuardSizes_ContainsExpectedValues()
    {
        Measurement.CommonGuardSizes.Should().Contain(new[]
        {
            "0", "0.5", "1", "1.5", "2", "3", "4", "5", "6", "7", "8",
            "Scissors", "Finger length", "Custom"
        });
    }

    [Fact]
    public void CommonTechniques_ContainsExpectedValues()
    {
        Measurement.CommonTechniques.Should().Contain(new[]
        {
            "Fade", "Taper", "Blend", "Scissor cut", "Clipper over comb",
            "Texturize", "Layer", "Thin out", "Square off", "Round off"
        });
    }

    [Fact]
    public void StepOrder_CanBeSetForWorkflowOrdering()
    {
        var measurements = new List<Measurement>
        {
            new() { Area = "Sides", StepOrder = 1 },
            new() { Area = "Back", StepOrder = 2 },
            new() { Area = "Top", StepOrder = 3 }
        };

        var ordered = measurements.OrderBy(m => m.StepOrder).ToList();

        ordered[0].Area.Should().Be("Sides");
        ordered[1].Area.Should().Be("Back");
        ordered[2].Area.Should().Be("Top");
    }
}
