using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Plans.Commands.DeletePlan;

public sealed record DeletePlanCommand(int PlanId) : IRequest<Result<Deleted>>;