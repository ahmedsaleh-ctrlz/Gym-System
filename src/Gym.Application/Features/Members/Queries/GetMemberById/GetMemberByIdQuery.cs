using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Members.Queries.GetMemberById;

public sealed record GetMemberByIdQuery(int id) : ICachedQuery<Result<MemberResponse>>
{
    public string cacheKey => $"Member_{id}";

    public string[] cacheTag => ["Member"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
