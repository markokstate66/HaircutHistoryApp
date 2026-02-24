using FluentAssertions;
using HaircutHistoryApp.Shared.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.Models;

public class ProfileTests
{
    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        var profile1 = new Profile();
        var profile2 = new Profile();

        profile1.Id.Should().NotBeNullOrEmpty();
        profile2.Id.Should().NotBeNullOrEmpty();
        profile1.Id.Should().NotBe(profile2.Id);
    }

    [Fact]
    public void Constructor_InitializesEmptyMeasurements()
    {
        var profile = new Profile();

        profile.Measurements.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var profile = new Profile();

        profile.Name.Should().BeEmpty();
        profile.OwnerUserId.Should().BeEmpty();
        profile.Description.Should().BeNull();
        profile.AvatarUrl.Should().BeNull();
        profile.ImageUrl1.Should().BeNull();
        profile.ImageUrl2.Should().BeNull();
        profile.ImageUrl3.Should().BeNull();
        profile.IsDeleted.Should().BeFalse();
        profile.ContentHash.Should().BeNull();
        profile.HaircutCount.Should().Be(0);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var profile = new Profile
        {
            Name = "Dad's winter haircut",
            OwnerUserId = "user-123",
            Description = "Shorter for cold weather",
            AvatarUrl = "https://example.com/avatar.jpg",
            ImageUrl1 = "https://example.com/img1.jpg",
            ImageUrl2 = "https://example.com/img2.jpg",
            ImageUrl3 = "https://example.com/img3.jpg",
            IsDeleted = false,
            ContentHash = "abc123",
            HaircutCount = 5
        };

        profile.Name.Should().Be("Dad's winter haircut");
        profile.OwnerUserId.Should().Be("user-123");
        profile.Description.Should().Be("Shorter for cold weather");
        profile.AvatarUrl.Should().Be("https://example.com/avatar.jpg");
        profile.ImageUrl1.Should().Be("https://example.com/img1.jpg");
        profile.ImageUrl2.Should().Be("https://example.com/img2.jpg");
        profile.ImageUrl3.Should().Be("https://example.com/img3.jpg");
        profile.HaircutCount.Should().Be(5);
    }

    [Fact]
    public void Measurements_CanBeAdded()
    {
        var profile = new Profile();
        profile.Measurements.Add(new Measurement { Area = "Top", GuardSize = "4", Technique = "Scissors" });
        profile.Measurements.Add(new Measurement { Area = "Sides", GuardSize = "2", Technique = "Fade" });

        profile.Measurements.Should().HaveCount(2);
        profile.Measurements[0].Area.Should().Be("Top");
        profile.Measurements[1].Area.Should().Be("Sides");
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedFlag()
    {
        var profile = new Profile { IsDeleted = false };

        profile.IsDeleted = true;

        profile.IsDeleted.Should().BeTrue();
    }
}
