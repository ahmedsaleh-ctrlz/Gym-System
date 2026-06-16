using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Coaches.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Coaches.Queries.GetCoachById;

public sealed record GetCoachByIdQuery(int Id) : ICachedQuery<Result<CoachResponse>>
{
    public string CacheKey => $"Coach_{Id}";

    public string[] CacheTag => ["Coach"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}