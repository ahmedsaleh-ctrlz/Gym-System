using Gym.Application.Common.Helpers;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Identity.Queries.GenerateToken;

public class GenerateTokenQueryHandler(
    ILogger<GenerateTokenQueryHandler> logger,
    ITokenProvider tokenProvider,
    IIdentityService identityService) : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(GenerateTokenQuery request, CancellationToken ct)
    {
        var userResult = await identityService.AuthenticateAsync(request.Email, request.Password, ct);

        if (userResult.IsError)
        {
            logger.LogWarning("Invalid Login Attemp for {Email} / Incorrect Email Or Password", Utility.MaskEmail(request.Email));
            return userResult.Errors;
        }

        var user = userResult.Value;

        return await tokenProvider.GenerateJwtTokenAsync(user, ct);
    }
}