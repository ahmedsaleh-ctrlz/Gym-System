using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Identity.Commands.Logout
{
    public sealed record RevokeRefreshTokenCommand(string RefreshToken) : IRequest<Result<Updated>>;
    public sealed class RevokeRefreshTokenCommandHandler(IAppDbContext dbContext) : IRequestHandler<RevokeRefreshTokenCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.Token == request.RefreshToken);
            if (refreshToken == null)
            {
                return Error.NotFound("Refresh_Token_Not_Found", "Refresh Token Not Found");
            }

            if(refreshToken.RevokedOnUtc != null)
            {
                return ApplicationErrors.RefreshTokenRevoked;
            }

            if (refreshToken.ExpiresOnUtc < DateTime.UtcNow)
            {
                return ApplicationErrors.RefreshTokenExpired;
            }

            refreshToken.Revoke();
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Updated;
        }
    }
}
