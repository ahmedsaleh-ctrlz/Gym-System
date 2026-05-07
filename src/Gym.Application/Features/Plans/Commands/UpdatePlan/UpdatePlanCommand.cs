using Gym.Domain.Common.Result;
using MediatR;

namespace Gym.Application.Features.Plans.Commands.UpdatePlan;

public sealed record UpdatePlanCommand(
    int PlanId,
    string Title,
    string? Description,
    decimal Cost,
    int DurationInDays) : IRequest<Result<Updated>>;
