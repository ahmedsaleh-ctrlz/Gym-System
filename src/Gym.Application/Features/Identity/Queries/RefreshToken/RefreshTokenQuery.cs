using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Identity.Queries.RefreshToken;

public sealed record RefreshTokenQuery(string RefreshToken, string ExpiredAccessToken) : IRequest<Result<TokenResponse>>;