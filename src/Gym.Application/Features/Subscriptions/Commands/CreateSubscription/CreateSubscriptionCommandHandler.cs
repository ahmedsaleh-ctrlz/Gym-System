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
        logger.LogTrace("Handling CreateSubscriptionCommand for member {MemberId} with plan {PlanId} starting on {StartDate}", request.memberId, request.plan.Id, request.startDate);
        var subscriptionResult = Subscription.Create(request.memberId, request.plan, request.startDate);
        if (subscriptionResult.IsError)
        {
            logger.LogError("Failed to create subscription for member {MemberId} with plan {PlanId}. Errors: {Errors}", request.memberId, request.plan.Id, subscriptionResult.Errors);
            return subscriptionResult.Errors;
        }

        await dbContext.Subscriptions.AddAsync(subscriptionResult.Value, ct);
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await dbContext.SaveChangesAsync(ct);

        return subscriptionResult.Value;
    }
}

