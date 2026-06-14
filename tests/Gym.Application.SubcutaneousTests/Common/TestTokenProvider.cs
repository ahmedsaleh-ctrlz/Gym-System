using System.Security.Claims;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class TestTokenProvider : ITokenProvider
{
    public ClaimsPrincipal? PrincipalFromExpiredToken { get; set; }
    public bool FailGenerateToken { get; set; }

    public Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default)
    {
        if (FailGenerateToken)
        {
            return Task.FromResult<Result<TokenResponse>>(Error.Failure("GenerateTokenFailed", "Generate token failed."));
        }

        return Task.FromResult<Result<TokenResponse>>(new TokenResponse
        {
            AccessToken = $"access-{user.UserId}",
            RefreshToken = $"refresh-{user.UserId}"
        });
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        return PrincipalFromExpiredToken;
    }
}
