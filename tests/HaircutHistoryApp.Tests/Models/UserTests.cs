using FluentAssertions;
using HaircutHistoryApp.Shared.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.Models;

public class UserTests
{
    [Fact]
    public void IsPremiumActive_WhenPremiumAndNoExpiry_ReturnsTrue()
    {
        var user = new User
        {
            IsPremium = true,
            PremiumExpiresAt = null
        };

        user.IsPremiumActive.Should().BeTrue();
    }

    [Fact]
    public void IsPremiumActive_WhenPremiumAndFutureExpiry_ReturnsTrue()
    {
        var user = new User
        {
            IsPremium = true,
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        user.IsPremiumActive.Should().BeTrue();
    }

    [Fact]
    public void IsPremiumActive_WhenPremiumButExpired_ReturnsFalse()
    {
        var user = new User
        {
            IsPremium = true,
            PremiumExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        user.IsPremiumActive.Should().BeFalse();
    }

    [Fact]
    public void IsPremiumActive_WhenNotPremium_ReturnsFalse()
    {
        var user = new User
        {
            IsPremium = false,
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        user.IsPremiumActive.Should().BeFalse();
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var user = new User();

        user.Id.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.DisplayName.Should().BeEmpty();
        user.IsPremium.Should().BeFalse();
        user.PremiumExpiresAt.Should().BeNull();
    }
}
