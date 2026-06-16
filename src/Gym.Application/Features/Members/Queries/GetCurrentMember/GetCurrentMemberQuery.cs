using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Members.Mappers;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Members.Queries.GetCurrentMember
{
    public sealed record GetCurrentMemberQuery() : IRequest<Result<MemberResponse>>;
}