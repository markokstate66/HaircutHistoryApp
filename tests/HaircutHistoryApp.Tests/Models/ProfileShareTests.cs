using FluentAssertions;
using HaircutHistoryApp.Shared.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.Models;

public class ProfileShareTests
{
    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        var share1 = new ProfileShare();
        var share2 = new ProfileShare();

        share1.Id.Should().NotBeNullOrEmpty();
        share2.Id.Should().NotBeNullOrEmpty();
        share1.Id.Should().NotBe(share2.Id);
    }

    [Fact]
    public void Constructor_DefaultsToActive()
    {
        var share = new ProfileShare();

        share.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var share = new ProfileShare();

        share.ProfileId.Should().BeEmpty();
        share.StylistUserId.Should().BeEmpty();
        share.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void Revoke_SetsInactiveAndRevokedTimestamp()
    {
        var share = new ProfileShare
        {
            IsActive = true,
            ProfileId = "profile-1",
            StylistUserId = "stylist-1"
        };

        share.IsActive = false;
        share.RevokedAt = DateTime.UtcNow;

        share.IsActive.Should().BeFalse();
        share.RevokedAt.Should().NotBeNull();
    }
}

public class ShareTokenTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var token = new ShareToken();

        token.Token.Should().BeEmpty();
        token.ShareUrl.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var token = new ShareToken
        {
            Token = "encrypted-token-123",
            ShareUrl = "haircuthistory://share/encrypted-token-123",
            ExpiresAt = expiresAt
        };

        token.Token.Should().Be("encrypted-token-123");
        token.ShareUrl.Should().Contain("haircuthistory://share/");
        token.ExpiresAt.Should().Be(expiresAt);
    }
}
