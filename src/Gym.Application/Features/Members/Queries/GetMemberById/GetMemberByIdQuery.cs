using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Members.Queries.GetMemberById;

public sealed record GetMemberByIdQuery(int Id) : ICachedQuery<Result<MemberResponse>>
{
    public string CacheKey => $"Member_{Id}";

    public string[] CacheTag => ["Member"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}