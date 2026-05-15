using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler(ILogger<Result<Subscription>> logger , IAppDbContext dbContext ,HybridCache cache) : IRequestHandler<CreateSubscriptionCommand, Result<Subscription>>
{
    public async Task<Result<Subscription>> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {
        var plan = await dbContext.Plans.FindAsync([request.planId], ct);
        if (plan is null)
        {
            logger.LogWarning("Plan with id {PlanId} not found for subscription creation.", request.planId);
            return Gym.Application.Common.Errors.ApplicationErrors.PlanNotFound;
        }

        logger.LogTrace("Handling CreateSubscriptionCommand for member {MemberId} with plan {PlanId} starting on {StartDate}", request.memberId, request.planId, request.startDate);
        var subscriptionResult = Subscription.Create(request.memberId, plan, request.startDate);
        if (subscriptionResult.IsError)
        {
            logger.LogError("Failed to create subscription for member {MemberId} with plan {PlanId}. Errors: {Errors}", request.memberId, request.planId, subscriptionResult.Errors);
            return subscriptionResult.Errors;
        }

        await dbContext.Subscriptions.AddAsync(subscriptionResult.Value, ct);
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await dbContext.SaveChangesAsync(ct);

        return subscriptionResult.Value;
    }
}

