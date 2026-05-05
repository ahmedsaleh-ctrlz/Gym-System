
using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Coaches.Dtos;
using Gym.Domain.Common.Result;


namespace Gym.Application.Features.Coaches.Queries.GetCoaches;

public sealed record GetCoachesQuery(int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    string? sortDirection = "Asce"
    ) : ICachedQuery<Result<PaginatedList<CoachResponse>>>
{
    public string cacheKey => $"Coaches:Page={PageNumber}:Size={PageSize}:Search={SearchTerm ?? "all"}:Sort={SortBy ?? "id"}:SortDirection={sortDirection}";    
    public string[] cacheTag => ["Coach"];
    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
