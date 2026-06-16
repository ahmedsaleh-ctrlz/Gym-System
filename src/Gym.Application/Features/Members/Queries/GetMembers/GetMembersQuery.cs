using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Members.Queries.GetMembers;

public sealed record GetMembersQuery(int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    string? SortDirection = "Asce"
    ) : ICachedQuery<Result<PaginatedList<MemberResponse>>>
{
    public string CacheKey => $"Members:Page={PageNumber}:Size={PageSize}:Search={SearchTerm ?? "all"}:Sort={SortBy ?? "id"}:SortDirection={SortDirection}";
    public string[] CacheTag => ["Member"];
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}