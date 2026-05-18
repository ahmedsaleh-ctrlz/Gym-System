using Gym.Application.Features.Attendances.Dtos;
using Gym.Domain.Common.Result;
using MediatR;

namespace Gym.Application.Features.Attendances.Commands.CheckInMember;

public sealed record CheckInMemberCommand(int MemberId) : IRequest<Result<AttendanceResponse>>;
