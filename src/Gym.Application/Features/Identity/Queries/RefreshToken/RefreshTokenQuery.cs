using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Identity.Queries.RefreshToken;

public sealed record RefreshTokenQuery(string RefreshToken, string ExpiredAccessToken) : IRequest<Result<TokenResponse>>;
