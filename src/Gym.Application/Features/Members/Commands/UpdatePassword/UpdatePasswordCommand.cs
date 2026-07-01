using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Members.Commands.UpdatePassword;

public sealed record UpdatePasswordCommand(int MemberId, string CurrentPassword, string NewPassword) : IRequest<Result<Updated>>;
