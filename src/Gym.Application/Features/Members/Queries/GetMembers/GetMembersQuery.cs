
using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;


namespace Gym.Application.Features.Members.Queries.GetMembers;

public sealed record GetMembersQuery(int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    string? sortDirection = "Asce"
    ) : ICachedQuery<Result<PaginatedList<MemberResponse>>>
{
    public string cacheKey => $"Members:Page={PageNumber}:Size={PageSize}:Search={SearchTerm ?? "all"}:Sort={SortBy ?? "id"}:SortDirection={sortDirection}";    
    public string[] cacheTag => ["Member"];
    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
