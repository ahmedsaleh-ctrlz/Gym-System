using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Plans.Commands.UpdatePlan;

public sealed class UpdatePlanCommandHandler(
    IAppDbContext context,
    ILogger<UpdatePlanCommandHandler> logger,
    HybridCache cache) : IRequestHandler<UpdatePlanCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdatePlanCommand command, CancellationToken ct)
    {
        logger.LogTrace("Handling {CommandName} for Plan ID {PlanId}.", nameof(UpdatePlanCommand), command.PlanId);

        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Id == command.PlanId, ct);
        if (plan is null)
        {
            logger.LogWarning("Plan with ID {PlanId} not found for update.", command.PlanId);
            return ApplicationErrors.PlanNotFound;
        }

        var updateResult = plan.UpdateInfo(command.Title, command.Description, command.Cost, command.DurationInDays);
        if (updateResult.IsError)
        {
            logger.LogWarning("Failed to update plan with ID {PlanId}. Errors: {Errors}", command.PlanId, updateResult.Errors);
            return updateResult.Errors;
        }

        await context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("Plan", ct);

        logger.LogInformation("Plan with ID {PlanId} updated successfully.", command.PlanId);

        return Result.Updated;
    }
}
