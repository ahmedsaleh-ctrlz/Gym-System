using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions;
using Gym.Tests.Common.Plans;

namespace Gym.Tests.Common.Subscriptions;

public static class SubscriptionFactory
{
    public static Result<Subscription> CreateSubscription(
        int? memberId = null,
        DateOnly? startDate = null,
        decimal? planCost = 500m,
        int? durationInDays = 30,
        int? allowedFreezeCount = 2,
        int? maxTotalFreezeDays = 14)
    {
        var planResult = PlanFactory.CreatePlan(
            cost: planCost,
            durationInDays: durationInDays,
            allowedFreezeCount: allowedFreezeCount,
            maxTotalFreezeDays: maxTotalFreezeDays);

        if (planResult.IsError)
        {
            return planResult.Errors;
        }

        return Subscription.Create(
            memberId ?? 1,
            planResult.Value,
            startDate ?? DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
