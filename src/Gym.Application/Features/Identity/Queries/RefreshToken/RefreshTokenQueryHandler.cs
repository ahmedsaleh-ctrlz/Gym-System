using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Gym.Application.Features.Identity.Queries.RefreshToken;

public sealed class RefreshTokenQueryHandler(
    ILogger<RefreshTokenQueryHandler> logger,
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    IAppDbContext context) : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery request, CancellationToken ct)
    {
        // Get principal From ExpiredToken
        var principal = tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

        if (principal is null)
        {
            logger.LogError("Expired access token is not valid");

            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            logger.LogError("Invalid userId claim");

            return ApplicationErrors.UserIdClaimInvalid;
        }

        // get user 

        var userResult = await identityService.GetUserByIdAsync(userId);

        if (userResult.IsError)
        {
            logger.LogError("Get user by id error occurred: {ErrorDescription}", userResult.TopError.Description);
            return userResult.Errors;
        }

        // Check Is Valid Refresh Token For User
        var refreshToken = context.RefreshTokens.FirstOrDefault(r => r.Token == request.RefreshToken && userId == r.UserId);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            logger.LogError("Refresh token has expired");

            return ApplicationErrors.RefreshTokenExpired;
        }

        // Create Another JWT TOKENS
        var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(userResult.Value, ct);

        if (generateTokenResult.IsError)
        {
            logger.LogError("Generate token error occurred: {ErrorDescription}", generateTokenResult.TopError.Description);

            return generateTokenResult.Errors;
        }

        return generateTokenResult.Value;
    }
}