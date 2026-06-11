using Gym.Domain.Common.Result;
using Gym.Domain.Identity;

namespace Gym.Tests.Common.Identity;

public static class RefreshTokenFactory
{
    public static Result<RefreshToken> CreateRefreshToken(
        string? token = "refresh-token",
        string? userId = "user-1",
        DateTimeOffset? expiresOnUtc = null)
    {
        return RefreshToken.Create(
            token,
            userId,
            expiresOnUtc ?? DateTimeOffset.UtcNow.AddDays(7));
    }
}
