using Gym.Domain.Common.Result;

using MediatR;
namespace Gym.Application.Features.Members.Commands.DeleteMember;

public sealed record DeleteMemberCommand(int MemberId) : IRequest<Result<Deleted>>;