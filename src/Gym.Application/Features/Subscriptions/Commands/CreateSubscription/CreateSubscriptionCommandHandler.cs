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

        if (request.StartDate > DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14))
        {
            logger.LogError("Start date {StartDate} is too far in the future for subscription creation for member {MemberId}.", request.StartDate, request.MemberId);
            return ApplicationErrors.InvalidSubscriptionStartDate;
        }

        if (await dbContext.Subscriptions.AnyAsync(s => s.MemberId == request.MemberId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Frozen)))
        {
            logger.LogError("Member {MemberId} already has an active or frozen subscription, cannot create new subscription.", request.MemberId);
            return ApplicationErrors.CannotCreateSubscriptionForMemberWithActiveOrFrozenSubscription;
        }

        logger.LogTrace("Handling CreateSubscriptionCommand for member {MemberId} with plan {PlanId} starting on {StartDate}", request.MemberId, request.PlanId, request.StartDate);
        var subscriptionResult = Subscription.Create(request.MemberId, plan, request.StartDate);
        if (subscriptionResult.IsError)
        {
            logger.LogError("Failed to create subscription for member {MemberId} with plan {PlanId}. Errors: {Errors}", request.MemberId, request.PlanId, subscriptionResult.Errors);
            return subscriptionResult.Errors;
        }

        var paymentResult = Payment.Create(subscriptionResult.Value);
        if (paymentResult.IsError)
        {
            logger.LogError("Failed to create payment for subscription of member {MemberId} with plan {PlanId}. Errors: {Errors}", request.MemberId, request.PlanId, paymentResult.Errors);
            return paymentResult.Errors;
        }

        await dbContext.Subscriptions.AddAsync(subscriptionResult.Value, ct);
        await dbContext.Payments.AddAsync(paymentResult.Value, ct);
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await cache.RemoveByTagAsync("Payments", ct);
        await cache.RemoveByTagAsync("AdminDashboard", ct);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Successfully created subscription with id {SubscriptionId} for member {MemberId} with plan {PlanId}.", subscriptionResult.Value.Id, request.MemberId, request.PlanId);

        return subscriptionResult.Value;
    }
}