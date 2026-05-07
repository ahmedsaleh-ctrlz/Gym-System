using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Plans.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Plans.Queries.GetPlans;

public sealed record GetPlansQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    string? SortDirection = "Asce") : ICachedQuery<Result<PaginatedList<PlanDetailsResponse>>>
{
    public string cacheKey =>
        $"Plans:Page={PageNumber}:Size={PageSize}:Search={SearchTerm ?? "all"}:Sort={SortBy ?? "id"}:SortDirection={SortDirection}";

    public string[] cacheTag => ["Plan"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
