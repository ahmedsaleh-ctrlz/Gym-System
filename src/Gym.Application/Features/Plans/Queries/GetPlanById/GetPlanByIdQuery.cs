using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Plans.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Plans.Queries.GetPlanById;

public sealed record GetPlanByIdQuery(int Id) : ICachedQuery<Result<PlanDetailsResponse>>
{
    public string cacheKey => $"Plan_{Id}";

    public string[] cacheTag => ["Plan"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
