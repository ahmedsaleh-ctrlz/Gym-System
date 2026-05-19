using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments;
using Gym.Domain.Subscriptions;
using Gym.Domain.Subscriptions.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler(ILogger<Result<Subscription>> logger, IAppDbContext dbContext, HybridCache cache) : IRequestHandler<CreateSubscriptionCommand, Result<Subscription>>
{
    public async Task<Result<Subscription>> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {

        #region CreateSubscription
        var plan = await dbContext.Plans.FindAsync([request.planId], ct);
        if (plan is null)
        {
            logger.LogWarning("Plan with id {PlanId} not found for subscription creation.", request.planId);
            return ApplicationErrors.PlanNotFound;
        }

        if (!plan.IsActive)
        {
            logger.LogError("Plan with id {PlanId} is not active for subscription creation.", request.planId);
            return ApplicationErrors.PlanNotActive;
        }

        var member = await dbContext.Members.FindAsync([request.memberId], ct);
        if (member is null)
        {
            logger.LogWarning("Member with id {MemberId} not found for subscription creation.", request.memberId);
            return ApplicationErrors.MemberNotFound;
        }

        if (request.startDate > DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14))
        {
            logger.LogError("Start date {StartDate} is too far in the future for subscription creation for member {MemberId}.", request.startDate, request.memberId);
            return ApplicationErrors.InvalidSubscriptionStartDate;
        }

        if (await dbContext.Subscriptions.AnyAsync(s => s.MemberId == request.memberId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Frozen)))
        {
            logger.LogError("Member {MemberId} already has an active or frozen subscription, cannot create new subscription.", request.memberId);
            return ApplicationErrors.CannotCreateSubscriptionForMemberWithActiveOrFrozenSubscription;
        }

        logger.LogTrace("Handling CreateSubscriptionCommand for member {MemberId} with plan {PlanId} starting on {StartDate}", request.memberId, request.planId, request.startDate);
        var subscriptionResult = Subscription.Create(request.memberId, plan, request.startDate);
        if (subscriptionResult.IsError)
        {
            logger.LogError("Failed to create subscription for member {MemberId} with plan {PlanId}. Errors: {Errors}", request.memberId, request.planId, subscriptionResult.Errors);
            return subscriptionResult.Errors;
        }
        #endregion

        #region CreatePayment
        var paymentResult = Payment.Create(subscriptionResult.Value);
        if (paymentResult.IsError)
        {
            logger.LogError("Failed to create payment for subscription of member {MemberId} with plan {PlanId}. Errors: {Errors}", request.memberId, request.planId, paymentResult.Errors);
            return paymentResult.Errors;
        }
        #endregion

        await dbContext.Subscriptions.AddAsync(subscriptionResult.Value, ct);
        await dbContext.Payments.AddAsync(paymentResult.Value, ct);
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await cache.RemoveByTagAsync("Payments", ct);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Successfully created subscription with id {SubscriptionId} for member {MemberId} with plan {PlanId}.", subscriptionResult.Value.Id, request.memberId, request.planId);

        return subscriptionResult.Value;
    }


   
}

