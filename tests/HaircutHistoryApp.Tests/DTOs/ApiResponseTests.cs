using FluentAssertions;
using HaircutHistoryApp.Shared.DTOs;
using HaircutHistoryApp.Shared.Models;
using Xunit;

namespace HaircutHistoryApp.Tests.DTOs;

public class ApiResponseGenericTests
{
    [Fact]
    public void Ok_CreatesSuccessfulResponse()
    {
        var profile = new Profile { Name = "Test Profile" };

        var response = ApiResponse<Profile>.Ok(profile);

        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be("Test Profile");
        response.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_CreatesErrorResponse()
    {
        var response = ApiResponse<Profile>.Fail(ErrorCodes.NotFound, "Profile not found");

        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be("NOT_FOUND");
        response.Error.Message.Should().Be("Profile not found");
    }

    [Fact]
    public void Ok_WithListData_ReturnsAllItems()
    {
        var profiles = new List<Profile>
        {
            new() { Name = "Profile 1" },
            new() { Name = "Profile 2" }
        };

        var response = ApiResponse<List<Profile>>.Ok(profiles);

        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
    }
}

public class ApiResponseTests
{
    [Fact]
    public void Ok_CreatesSuccessfulResponse()
    {
        var response = ApiResponse.Ok();

        response.Success.Should().BeTrue();
        response.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_CreatesErrorResponse()
    {
        var response = ApiResponse.Fail(ErrorCodes.Unauthorized, "Invalid token");

        response.Success.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error!.Code.Should().Be("UNAUTHORIZED");
        response.Error.Message.Should().Be("Invalid token");
    }
}

public class ErrorCodesTests
{
    [Theory]
    [InlineData(nameof(ErrorCodes.Unauthorized), "UNAUTHORIZED")]
    [InlineData(nameof(ErrorCodes.Forbidden), "FORBIDDEN")]
    [InlineData(nameof(ErrorCodes.NotFound), "NOT_FOUND")]
    [InlineData(nameof(ErrorCodes.LimitExceeded), "LIMIT_EXCEEDED")]
    [InlineData(nameof(ErrorCodes.PremiumRequired), "PREMIUM_REQUIRED")]
    [InlineData(nameof(ErrorCodes.ValidationError), "VALIDATION_ERROR")]
    [InlineData(nameof(ErrorCodes.TokenExpired), "TOKEN_EXPIRED")]
    [InlineData(nameof(ErrorCodes.InternalError), "INTERNAL_ERROR")]
    public void ErrorCodes_HaveExpectedValues(string fieldName, string expectedValue)
    {
        var field = typeof(ErrorCodes).GetField(fieldName);
        field.Should().NotBeNull();
        field!.GetValue(null).Should().Be(expectedValue);
    }
}

public class PaginatedResponseTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        var response = new PaginatedResponse<Profile>();

        response.Data.Should().NotBeNull().And.BeEmpty();
        response.Total.Should().Be(0);
        response.Limit.Should().Be(0);
        response.Offset.Should().Be(0);
        response.Success.Should().BeTrue();
    }

    [Fact]
    public void Properties_TrackPagination()
    {
        var response = new PaginatedResponse<Profile>
        {
            Data = new List<Profile> { new() { Name = "Profile 1" } },
            Total = 25,
            Limit = 10,
            Offset = 10
        };

        response.Data.Should().HaveCount(1);
        response.Total.Should().Be(25);
        response.Limit.Should().Be(10);
        response.Offset.Should().Be(10);
    }
}
