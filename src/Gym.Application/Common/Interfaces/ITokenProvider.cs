using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;

using Microsoft.IdentityModel.Tokens;

namespace Gym.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}