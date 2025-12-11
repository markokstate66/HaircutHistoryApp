using FluentAssertions;
using HaircutHistoryApp.Core.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.Models;

public class AchievementTests
{
    [Fact]
    public void Progress_WhenCurrentValueIsZero_ReturnsZero()
    {
        var achievement = new Achievement
        {
            TargetValue = 10,
            CurrentValue = 0
        };

        achievement.Progress.Should().Be(0);
    }

    [Fact]
    public void Progress_WhenCurrentValueIsHalfOfTarget_ReturnsFiftyPercent()
    {
        var achievement = new Achievement
        {
            TargetValue = 10,
            CurrentValue = 5
        };

        achievement.Progress.Should().Be(0.5);
    }

    [Fact]
    public void Progress_WhenCurrentValueExceedsTarget_ReturnsOne()
    {
        var achievement = new Achievement
        {
            TargetValue = 10,
            CurrentValue = 15
        };

        achievement.Progress.Should().Be(1.0);
    }

    [Fact]
    public void Progress_WhenTargetIsZero_ReturnsZero()
    {
        var achievement = new Achievement
        {
            TargetValue = 0,
            CurrentValue = 5
        };

        achievement.Progress.Should().Be(0);
    }

    [Fact]
    public void ProgressText_ReturnsCorrectFormat()
    {
        var achievement = new Achievement
        {
            TargetValue = 10,
            CurrentValue = 3
        };

        achievement.ProgressText.Should().Be("3/10");
    }
}

public class AchievementDefinitionsTests
{
    [Fact]
    public void GetAll_Returns22Achievements()
    {
        var achievements = AchievementDefinitions.GetAll();

        achievements.Should().HaveCount(22);
    }

    [Fact]
    public void GetClientAchievements_Returns15Achievements()
    {
        var achievements = AchievementDefinitions.GetClientAchievements();

        achievements.Should().HaveCount(15);
        achievements.Should().NotContain(a => a.Category == AchievementCategory.BarberMode);
    }

    [Fact]
    public void GetBarberAchievements_Returns7Achievements()
    {
        var achievements = AchievementDefinitions.GetBarberAchievements();

        achievements.Should().HaveCount(7);
        achievements.Should().OnlyContain(a => a.Category == AchievementCategory.BarberMode);
    }

    [Fact]
    public void GetAll_AllAchievementsHaveUniqueIds()
    {
        var achievements = AchievementDefinitions.GetAll();
        var ids = achievements.Select(a => a.Id).ToList();

        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetAll_AllAchievementsHaveRequiredFields()
    {
        var achievements = AchievementDefinitions.GetAll();

        achievements.Should().AllSatisfy(a =>
        {
            a.Id.Should().NotBeNullOrEmpty();
            a.Name.Should().NotBeNullOrEmpty();
            a.Description.Should().NotBeNullOrEmpty();
            a.Icon.Should().NotBeNullOrEmpty();
            a.TargetValue.Should().BeGreaterThan(0);
        });
    }

    [Theory]
    [InlineData("HAIRCUT_1", 1)]
    [InlineData("HAIRCUT_5", 5)]
    [InlineData("HAIRCUT_10", 10)]
    [InlineData("HAIRCUT_25", 25)]
    [InlineData("HAIRCUT_50", 50)]
    [InlineData("HAIRCUT_100", 100)]
    public void HaircutAchievements_HaveCorrectTargetValues(string id, int expectedTarget)
    {
        var achievements = AchievementDefinitions.GetAll();
        var achievement = achievements.FirstOrDefault(a => a.Id == id);

        achievement.Should().NotBeNull();
        achievement!.TargetValue.Should().Be(expectedTarget);
        achievement.Category.Should().Be(AchievementCategory.Haircuts);
    }

    [Theory]
    [InlineData("VISIT_1", 1)]
    [InlineData("VISIT_5", 5)]
    [InlineData("VISIT_10", 10)]
    [InlineData("VISIT_25", 25)]
    [InlineData("VISIT_50", 50)]
    [InlineData("VISIT_100", 100)]
    public void BarberVisitAchievements_HaveCorrectTargetValues(string id, int expectedTarget)
    {
        var achievements = AchievementDefinitions.GetAll();
        var achievement = achievements.FirstOrDefault(a => a.Id == id);

        achievement.Should().NotBeNull();
        achievement!.TargetValue.Should().Be(expectedTarget);
        achievement.Category.Should().Be(AchievementCategory.BarberVisits);
    }
}
