using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Plans.Commands.DeletePlan;

public sealed class DeletePlanCommandHandler(
    IAppDbContext context,
    ILogger<DeletePlanCommandHandler> logger,
    HybridCache cache) : IRequestHandler<DeletePlanCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeletePlanCommand command, CancellationToken ct)
    {
        logger.LogTrace("Handling {CommandName} for Plan ID {PlanId}.", nameof(DeletePlanCommand), command.PlanId);

        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Id == command.PlanId, ct);
        if (plan is null)
        {
            logger.LogWarning("Plan with ID {PlanId} not found for delete.", command.PlanId);
            return ApplicationErrors.PlanNotFound;
        }

        var deleteResult = plan.Deactivate();
        if (deleteResult.IsError)
        {
            logger.LogWarning("Failed to deactivate plan with ID {PlanId}. Errors: {Errors}", command.PlanId, deleteResult.Errors);
            return deleteResult.Errors;
        }

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("Plan", ct);

        logger.LogInformation("Plan with ID {PlanId} deactivated successfully.", command.PlanId);

        return Result.Deleted;
    }
}