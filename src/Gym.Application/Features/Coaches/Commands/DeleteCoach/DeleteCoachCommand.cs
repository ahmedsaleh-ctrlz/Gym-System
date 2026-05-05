using Gym.Domain.Common.Result;
using MediatR;
namespace Gym.Application.Features.Coaches.Commands.DeleteCoach;
public sealed record DeleteCoachCommand(int CoachId) : IRequest<Result<Deleted>>;
