using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Subscriptions.Commands.UpdateSubscriptionStatus;

public sealed class UpdateSubscriptionStatusCommandHandler(ILogger<Result<Updated>> logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<UpdateSubscriptionStatusCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateSubscriptionStatusCommand request, CancellationToken ct)
    {
        logger.LogTrace("Handling UpdateSubscriptionStatusCommand for SubscriptionId: {SubscriptionId}", request.SubscriptionId);
        var subscription = await context.Subscriptions.FindAsync([request.SubscriptionId], ct);
        if (subscription is null)
        {
            logger.LogWarning("Subscription with Id {SubscriptionId} not found", request.SubscriptionId);
            return ApplicationErrors.SubscriptionNotFound;
        }

        switch (request.NewStatus)
        {
            case SubscriptionStatus.Active:
                var activateResult = subscription.Activate();
                if (activateResult.IsError)
                {
                    logger.LogWarning("Failed to activate subscription with Id {SubscriptionId}: {Errors}", request.SubscriptionId, activateResult.Errors);
                    return activateResult.Errors;
                }

                break;

            case SubscriptionStatus.Scheduled:
                var scheduleResult = subscription.Scheduled();
                if (scheduleResult.IsError)
                {
                    logger.LogWarning("Failed to schedule subscription with Id {SubscriptionId}: {Errors}", request.SubscriptionId, scheduleResult.Errors);
                    return scheduleResult.Errors;
                }

                break;

            case SubscriptionStatus.Cancelled:
                var cancelResult = subscription.Cancel();
                if (cancelResult.IsError)
                {
                    logger.LogWarning("Failed to cancel subscription with Id {SubscriptionId}: {Errors}", request.SubscriptionId, cancelResult.Errors);
                    return cancelResult.Errors;
                }

                break;
        }

        logger.LogInformation("Subscription with Id {SubscriptionId} status updated to {NewStatus}", request.SubscriptionId, request.NewStatus);
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await cache.RemoveByTagAsync("AdminDashboard", ct);
        await context.SaveChangesAsync(ct);

        return Result.Updated;
    }
}