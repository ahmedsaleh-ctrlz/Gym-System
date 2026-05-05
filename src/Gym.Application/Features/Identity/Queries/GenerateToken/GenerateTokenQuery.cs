using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using MediatR;


namespace Gym.Application.Features.Identity.Queries.GenerateToken;

public sealed record GenerateTokenQuery(string email,
    string password) : IRequest<Result<TokenResponse>>;
