using Gym.Domain.Common;
using Gym.Domain.Common.Result;

namespace Gym.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string? Token { get; }
    public string? UserId { get; }
    public DateTimeOffset ExpiresOnUtc { get; }

    public DateTimeOffset? RevokedOnUtc { get; private set; } = null;

    private RefreshToken()
    { }

    private RefreshToken(string? token, string? userId, DateTimeOffset expiresOnUtc)
    {
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
        RevokedOnUtc = null;
    }

    public static Result<RefreshToken> Create(string? token, string? userId, DateTimeOffset expiresOnUtc)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return RefreshTokenErrors.TokenRequired;
        }

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

    public void Revoke()
    {
        RevokedOnUtc = DateTime.UtcNow;
    }

    public static string HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return string.Empty;
        }

        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}