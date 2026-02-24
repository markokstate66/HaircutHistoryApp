using FluentAssertions;
using HaircutHistoryApp.Shared.DTOs;
using Xunit;

namespace HaircutHistoryApp.Tests.DTOs;

public class ProfileSyncInfoTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        var syncInfo = new ProfileSyncInfo();

        syncInfo.Id.Should().BeEmpty();
        syncInfo.ContentHash.Should().BeEmpty();
        syncInfo.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CanDetectDeletedProfiles()
    {
        var syncInfo = new ProfileSyncInfo
        {
            Id = "profile-1",
            ContentHash = "hash-123",
            IsDeleted = true,
            UpdatedAt = DateTime.UtcNow
        };

        syncInfo.IsDeleted.Should().BeTrue();
    }
}

public class SyncResponseTests
{
    [Fact]
    public void Constructor_InitializesEmptyProfilesList()
    {
        var response = new SyncResponse();

        response.Profiles.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CanContainMultipleProfileSyncInfos()
    {
        var response = new SyncResponse
        {
            ServerTime = DateTime.UtcNow,
            Profiles = new List<ProfileSyncInfo>
            {
                new() { Id = "p1", ContentHash = "hash1" },
                new() { Id = "p2", ContentHash = "hash2" },
                new() { Id = "p3", ContentHash = "hash3", IsDeleted = true }
            }
        };

        response.Profiles.Should().HaveCount(3);
        response.Profiles.Count(p => p.IsDeleted).Should().Be(1);
    }
}

public class BatchFetchRequestTests
{
    [Fact]
    public void Constructor_InitializesEmptyIds()
    {
        var request = new BatchFetchRequest();

        request.Ids.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CanContainMultipleIds()
    {
        var request = new BatchFetchRequest
        {
            Ids = new List<string> { "id-1", "id-2", "id-3" }
        };

        request.Ids.Should().HaveCount(3);
    }
}

public class HaircutRecordSyncInfoTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        var syncInfo = new HaircutRecordSyncInfo();

        syncInfo.Id.Should().BeEmpty();
        syncInfo.ProfileId.Should().BeEmpty();
        syncInfo.ContentHash.Should().BeEmpty();
        syncInfo.IsDeleted.Should().BeFalse();
    }
}

public class BatchRecordFetchRequestTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        var request = new BatchRecordFetchRequest();

        request.ProfileId.Should().BeEmpty();
        request.Ids.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CanSpecifyProfileAndRecordIds()
    {
        var request = new BatchRecordFetchRequest
        {
            ProfileId = "profile-1",
            Ids = new List<string> { "record-1", "record-2" }
        };

        request.ProfileId.Should().Be("profile-1");
        request.Ids.Should().HaveCount(2);
    }
}
