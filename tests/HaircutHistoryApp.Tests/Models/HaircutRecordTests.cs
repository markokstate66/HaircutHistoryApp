using FluentAssertions;
using HaircutHistoryApp.Shared.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.Models;

public class HaircutRecordTests
{
    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        var record1 = new HaircutRecord();
        var record2 = new HaircutRecord();

        record1.Id.Should().NotBeNullOrEmpty();
        record2.Id.Should().NotBeNullOrEmpty();
        record1.Id.Should().NotBe(record2.Id);
    }

    [Fact]
    public void Constructor_InitializesEmptyPhotoUrls()
    {
        var record = new HaircutRecord();

        record.PhotoUrls.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var record = new HaircutRecord();

        record.ProfileId.Should().BeEmpty();
        record.CreatedByUserId.Should().BeEmpty();
        record.StylistName.Should().BeNull();
        record.Location.Should().BeNull();
        record.Notes.Should().BeNull();
        record.Price.Should().BeNull();
        record.DurationMinutes.Should().BeNull();
        record.IsDeleted.Should().BeFalse();
        record.ContentHash.Should().BeNull();
    }

    [Fact]
    public void DisplaySummary_WhenNotesExist_ShowsNotes()
    {
        var record = new HaircutRecord
        {
            Notes = "Great cut, asked for extra texture on top",
            Date = new DateTime(2026, 1, 15)
        };

        record.DisplaySummary.Should().Be("Great cut, asked for extra texture on top");
    }

    [Fact]
    public void DisplaySummary_WhenNoNotes_ShowsFormattedDate()
    {
        var record = new HaircutRecord
        {
            Date = new DateTime(2026, 1, 15)
        };

        record.DisplaySummary.Should().Be("Haircut on Jan 15, 2026");
    }

    [Fact]
    public void DisplaySummary_WhenEmptyNotes_ShowsFormattedDate()
    {
        var record = new HaircutRecord
        {
            Notes = "",
            Date = new DateTime(2026, 6, 1)
        };

        record.DisplaySummary.Should().Be("Haircut on Jun 1, 2026");
    }

    [Fact]
    public void Properties_CanStoreFullRecord()
    {
        var record = new HaircutRecord
        {
            ProfileId = "profile-1",
            CreatedByUserId = "user-1",
            Date = new DateTime(2026, 2, 10),
            StylistName = "John",
            Location = "Downtown Barber",
            Notes = "Went a bit shorter on the sides",
            Price = 35.00m,
            DurationMinutes = 45,
            PhotoUrls = new List<string> { "photo1.jpg", "photo2.jpg" }
        };

        record.ProfileId.Should().Be("profile-1");
        record.StylistName.Should().Be("John");
        record.Location.Should().Be("Downtown Barber");
        record.Price.Should().Be(35.00m);
        record.DurationMinutes.Should().Be(45);
        record.PhotoUrls.Should().HaveCount(2);
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedFlag()
    {
        var record = new HaircutRecord { IsDeleted = false };

        record.IsDeleted = true;

        record.IsDeleted.Should().BeTrue();
    }
}
