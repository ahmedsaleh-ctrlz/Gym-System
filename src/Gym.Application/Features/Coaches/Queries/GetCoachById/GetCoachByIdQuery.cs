using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Coaches.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Coaches.Queries.GetCoachById;

public sealed record GetCoachByIdQuery(int id) : ICachedQuery<Result<CoachResponse>>
{
    public string cacheKey => $"Coach_{id}";

    public string[] cacheTag => ["Coach"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
