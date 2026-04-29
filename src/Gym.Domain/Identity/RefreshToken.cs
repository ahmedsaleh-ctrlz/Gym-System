using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;

namespace Gym.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string? Token { get; }
    public string? UserId { get; }
    public DateTimeOffset ExpiresOnUtc { get; }

    private RefreshToken()
    { }

    private RefreshToken(string? token, string? userId, DateTimeOffset expiresOnUtc) 
    {
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
    }

    public static Result<RefreshToken> Create(string? token, string? userId, DateTimeOffset expiresOnUtc)
    {
       

        if (string.IsNullOrWhiteSpace(userId))
        {
            return RefreshTokenErrors.UserIdRequired;
        }

        if (expiresOnUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenErrors.ExpiryInvalid;
        }

        return new RefreshToken(token, userId, expiresOnUtc);
    }
}