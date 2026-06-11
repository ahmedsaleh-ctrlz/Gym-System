using Gym.Domain.Identity;
using Gym.Tests.Common.Identity;

namespace Gym.Domain.UnitTests.Identity;

public class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenTokenIsMissing()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(token: "");

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.TokenRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenExpiryIsInPast()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(expiresOnUtc: DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.ExpiryInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var expiresOnUtc = DateTimeOffset.UtcNow.AddDays(2);

        var result = RefreshTokenFactory.CreateRefreshToken(expiresOnUtc: expiresOnUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal("refresh-token", result.Value.Token);
        Assert.Equal("user-1", result.Value.UserId);
        Assert.Equal(expiresOnUtc, result.Value.ExpiresOnUtc);
    }
}
