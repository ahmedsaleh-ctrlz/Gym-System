using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;


namespace Gym.Application.Features.Subscriptions.Commands.FreezeSubscription;

public class FreezeSubscriptionCommandHandler(IAppDbContext context,
    ILogger<Result<Updated>> logger,
    HybridCache cache) : IRequestHandler<FreezeSubscriptionCommand, Result<Updated>>
{
  
    public async Task<Result<Updated>> Handle(FreezeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        logger.LogTrace("handle Freezing subscription with id {subscriptionId} for {FreezeDays} days", request.subscriptionId, request.FreezeDays);
        var subscription = await context.Subscriptions.Include(s=> s.Plan).FirstOrDefaultAsync(s => s.Id == request.subscriptionId, cancellationToken);
        if(subscription is null)
            return ApplicationErrors.SubscriptionNotFound;
        var FreezeResult = subscription.Freeze(request.FreezeDays);
        if (FreezeResult.IsError) {
            logger.LogWarning("Subscription with id {subscriptionId} cannot be frozen for {FreezeDays} days. Errors: {Errors}", request.subscriptionId, request.FreezeDays, FreezeResult.Errors);
            return FreezeResult.Errors;
        }
        logger.LogInformation("Subscription with id {subscriptionId} frozen for {FreezeDays} days successfully", request.subscriptionId, request.FreezeDays);
        await cache.RemoveByTagAsync($"Subscriptions", cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Updated;
    }

   

}   

