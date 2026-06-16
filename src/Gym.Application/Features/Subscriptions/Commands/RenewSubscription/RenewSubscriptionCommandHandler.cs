using FluentValidation;

using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments;
using Gym.Domain.Subscriptions;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Subscriptions.Commands.RenewSubscription;

public sealed class RenewSubscriptionCommandHandler(ILogger<Result<Created>> logger, IAppDbContext dbContext, HybridCache cache) : IRequestHandler<RenewSubscriptionCommand, Result<Created>>
{
    public async Task<Result<Created>> Handle(RenewSubscriptionCommand request, CancellationToken ct)
    {
        var plan = await dbContext.Plans.FindAsync([request.PlanId], ct);
        if (plan is null)
        {
            logger.LogWarning("Plan with id {PlanId} not found for subscription creation.", request.PlanId);
            return ApplicationErrors.PlanNotFound;
        }

        if (!plan.IsActive)
        {
            logger.LogError("Plan with id {PlanId} is not active for subscription creation.", request.PlanId);
            return ApplicationErrors.PlanNotActive;
        }

        var member = await dbContext.Members.FindAsync([request.MemberId], ct);
        if (member is null)
        {
            logger.LogWarning("Member with id {MemberId} not found for subscription creation.", request.MemberId);
            return ApplicationErrors.MemberNotFound;
        }

        var existingSubscription = await dbContext.Subscriptions.Where(s => s.MemberId == request.MemberId).OrderByDescending(s => s.EndDate).FirstOrDefaultAsync(ct);

        if (existingSubscription is null)
        {
            logger.LogWarning("No existing subscription found for member {MemberId}. Proceeding with new subscription creation.", request.MemberId);
            return ApplicationErrors.SubscriptionNotFound;
        }

        var startDate = existingSubscription.EndDate.AddDays(1);

        logger.LogTrace("Handling RenewSubscriptionCommand for member {MemberId} with plan {PlanId} starting on {StartDate}", request.MemberId, request.PlanId, startDate);
        var subscriptionResult = Subscription.Create(request.MemberId, plan, startDate);
        if (subscriptionResult.IsError)
        {
            logger.LogError("Failed to create subscription for member {MemberId} with plan {PlanId}. Errors: {Errors}", request.MemberId, request.PlanId, subscriptionResult.Errors);
            return subscriptionResult.Errors;
        }

        var subscription = subscriptionResult.Value;
        var paymentResult = Payment.Create(subscriptionResult.Value);
        if (paymentResult.IsError)
        {
            logger.LogError("Failed to create payment for subscription of member {MemberId} with plan {PlanId}. Errors: {Errors}", request.MemberId, request.PlanId, paymentResult.Errors);
            return paymentResult.Errors;
        }

        await dbContext.Subscriptions.AddAsync(subscription, ct);
        await dbContext.Payments.AddAsync(paymentResult.Value, ct);
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await cache.RemoveByTagAsync("Payments", ct);
        await cache.RemoveByTagAsync("AdminDashboard", ct);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Successfully renewed subscription with id {SubscriptionId} for member {MemberId} with plan {PlanId}.", subscription.Id, request.MemberId, request.PlanId);

        return Result.Created;
    }
}