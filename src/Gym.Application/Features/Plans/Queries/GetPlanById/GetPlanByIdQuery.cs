using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Plans.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Plans.Queries.GetPlanById;

public sealed record GetPlanByIdQuery(int Id) : ICachedQuery<Result<PlanDetailsResponse>>
{
    public string CacheKey => $"Plan_{Id}";

    public string[] CacheTag => ["Plan"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}