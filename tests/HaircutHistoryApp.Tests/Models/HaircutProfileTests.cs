using FluentAssertions;
using HaircutHistoryApp.Core.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.Models;

public class HaircutProfileTests
{
    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        var profile1 = new HaircutProfile();
        var profile2 = new HaircutProfile();

        profile1.Id.Should().NotBeNullOrEmpty();
        profile2.Id.Should().NotBeNullOrEmpty();
        profile1.Id.Should().NotBe(profile2.Id);
    }

    [Fact]
    public void Constructor_SetsCreatedAtToNow()
    {
        var before = DateTime.UtcNow;
        var profile = new HaircutProfile();
        var after = DateTime.UtcNow;

        profile.CreatedAt.Should().BeOnOrAfter(before);
        profile.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_InitializesEmptyCollections()
    {
        var profile = new HaircutProfile();

        profile.Measurements.Should().NotBeNull().And.BeEmpty();
        profile.LocalImagePaths.Should().NotBeNull().And.BeEmpty();
        profile.ImageUrls.Should().NotBeNull().And.BeEmpty();
        profile.BarberNotes.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void PrimaryImage_WhenLocalImageExists_ReturnsLocalPath()
    {
        var profile = new HaircutProfile
        {
            LocalImagePaths = new List<string> { "/local/image1.jpg", "/local/image2.jpg" }
        };

        profile.PrimaryImage.Should().Be("/local/image1.jpg");
    }

    [Fact]
    public void PrimaryImage_WhenNoLocalButUrlExists_ReturnsUrl()
    {
        var profile = new HaircutProfile
        {
            LocalImagePaths = new List<string>(),
            ImageUrls = new List<string> { "https://example.com/image1.jpg" }
        };

        profile.PrimaryImage.Should().Be("https://example.com/image1.jpg");
    }

    [Fact]
    public void PrimaryImage_WhenNoImages_ReturnsNull()
    {
        var profile = new HaircutProfile();

        profile.PrimaryImage.Should().BeNull();
    }

    [Fact]
    public void HasImages_WhenLocalImagesExist_ReturnsTrue()
    {
        var profile = new HaircutProfile
        {
            LocalImagePaths = new List<string> { "/local/image.jpg" }
        };

        profile.HasImages.Should().BeTrue();
    }

    [Fact]
    public void HasImages_WhenUrlImagesExist_ReturnsTrue()
    {
        var profile = new HaircutProfile
        {
            ImageUrls = new List<string> { "https://example.com/image.jpg" }
        };

        profile.HasImages.Should().BeTrue();
    }

    [Fact]
    public void HasImages_WhenNoImages_ReturnsFalse()
    {
        var profile = new HaircutProfile();

        profile.HasImages.Should().BeFalse();
    }
}

public class HaircutMeasurementTests
{
    [Fact]
    public void CommonAreas_ContainsExpectedValues()
    {
        HaircutMeasurement.CommonAreas.Should().Contain(new[]
        {
            "Top", "Sides", "Back", "Neckline", "Sideburns",
            "Bangs/Fringe", "Crown", "Beard", "Mustache"
        });
    }

    [Fact]
    public void CommonGuardSizes_ContainsExpectedValues()
    {
        HaircutMeasurement.CommonGuardSizes.Should().Contain(new[]
        {
            "0", "0.5", "1", "1.5", "2", "3", "4", "5", "6", "7", "8",
            "Scissors", "Finger length", "Custom"
        });
    }

    [Fact]
    public void CommonTechniques_ContainsExpectedValues()
    {
        HaircutMeasurement.CommonTechniques.Should().Contain(new[]
        {
            "Fade", "Taper", "Blend", "Scissor cut", "Clipper over comb",
            "Texturize", "Layer", "Thin out", "Square off", "Round off"
        });
    }

    [Fact]
    public void DisplayText_WithAllFields_FormatsCorrectly()
    {
        var measurement = new HaircutMeasurement
        {
            Area = "Top",
            GuardSize = "4",
            Technique = "Scissors"
        };

        measurement.DisplayText.Should().Be("Top: #4 (Scissors)");
    }

    [Fact]
    public void DisplayText_WithNoGuardSize_FormatsCorrectly()
    {
        var measurement = new HaircutMeasurement
        {
            Area = "Top",
            GuardSize = "",
            Technique = "Scissors"
        };

        measurement.DisplayText.Should().Be("Top:  (Scissors)");
    }
}
