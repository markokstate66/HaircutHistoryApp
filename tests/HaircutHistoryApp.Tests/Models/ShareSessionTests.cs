using FluentAssertions;
using HaircutHistoryApp.Core.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.Models;

public class ShareSessionTests
{
    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        var session1 = new ShareSession();
        var session2 = new ShareSession();

        session1.Id.Should().NotBeNullOrEmpty();
        session2.Id.Should().NotBeNullOrEmpty();
        session1.Id.Should().NotBe(session2.Id);
    }

    [Fact]
    public void Constructor_GeneratesShareCode()
    {
        var session = new ShareSession();

        session.ShareCode.Should().NotBeNullOrEmpty();
        session.ShareCode.Should().HaveLength(8);
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInFuture_ReturnsFalse()
    {
        var session = new ShareSession
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        session.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsInPast_ReturnsTrue()
    {
        var session = new ShareSession
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        session.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtIsNow_ReturnsTrue()
    {
        var session = new ShareSession
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(-1)
        };

        session.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void ShareCode_IsAlphanumericUppercase()
    {
        var session = new ShareSession();

        session.ShareCode.Should().MatchRegex("^[A-Z0-9]{8}$");
    }

    [Fact]
    public void MultipleShareCodes_AreUnique()
    {
        var codes = Enumerable.Range(0, 100)
            .Select(_ => new ShareSession().ShareCode)
            .ToList();

        codes.Should().OnlyHaveUniqueItems();
    }
}
