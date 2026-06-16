using Gym.Domain.Subscriptions;
using Gym.Domain.Subscriptions.Enums;
using Gym.Tests.Common.Subscriptions;

namespace Gym.Domain.UnitTests.Subscriptions;

public class SubscriptionTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenMemberIdIsInvalid()
    {
        var result = SubscriptionFactory.CreateSubscription(memberId: 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.SubscriptionShouldAssignToMember.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenPlanIdIsInvalid()
    {
        var plan = Gym.Tests.Common.Plans.PlanFactory.CreatePlan(id: 0).Value;

        var result = Subscription.Create(1, plan, DateOnly.FromDateTime(DateTime.UtcNow));

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.SubscriptionShouldAssignToPlan.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenStartDateIsInPast()
    {
        var result = SubscriptionFactory.CreateSubscription(startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.SubscriptionStartDateCannotBeInThePast.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var subscription = SubscriptionFactory.CreateSubscription(memberId: 7, startDate: today, planCost: 600m, durationInDays: 30).Value;

        Assert.Equal(7, subscription.MemberId);
        Assert.Equal(600m, subscription.PriceSnapshot);
        Assert.Equal(today, subscription.StartDate);
        Assert.Equal(today.AddDays(30), subscription.EndDate);
        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
        Assert.Equal(0, subscription.FreezeCountUsed);
        Assert.Equal(0, subscription.TotalFreezeDaysUsed);
    }

    [Fact]
    public void Activate_ShouldReturnSuccess_WhenStatusIsPending()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.Activate();

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    [Fact]
    public void Activate_ShouldReturnSuccess_WhenStatusIsScheduled()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Scheduled();

        var result = subscription.Activate();

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    [Fact]
    public void Activate_ShouldReturnSuccess_WhenStatusIsFrozen()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();
        subscription.Freeze(0);

        var result = subscription.Activate();

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    [Fact]
    public void Activate_ShouldReturnError_WhenStatusCannotBeActivated()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Cancel();

        var result = subscription.Activate();

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.InvalidStatusCannotActivate.Code, result.TopError.Code);
    }

    [Fact]
    public void Scheduled_ShouldReturnSuccess_WhenStatusIsPending()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.Scheduled();

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Scheduled, subscription.Status);
    }

    [Fact]
    public void Scheduled_ShouldReturnError_WhenStatusIsNotPending()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();

        var result = subscription.Scheduled();

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.OnlyPendingSubscriptionsCanBeScheduled.Code, result.TopError.Code);
    }

    [Fact]
    public void Freeze_ShouldReturnSuccess_WhenSubscriptionIsActive()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();
        var originalEndDate = subscription.EndDate;

        var result = subscription.Freeze(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Frozen, subscription.Status);
        Assert.Equal(1, subscription.FreezeCountUsed);
        Assert.Equal(5, subscription.TotalFreezeDaysUsed);
        Assert.Equal(originalEndDate.AddDays(5), subscription.EndDate);
    }

    [Fact]
    public void Freeze_ShouldReturnError_WhenSubscriptionIsNotActive()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.Freeze(5);

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.OnlyActiveSubscriptionsCanBeFrozen.Code, result.TopError.Code);
    }

    [Fact]
    public void Freeze_ShouldReturnError_WhenFreezeCountExceedsPlanLimit()
    {
        var subscription = SubscriptionFactory.CreateSubscription(allowedFreezeCount: 1, maxTotalFreezeDays: 20).Value;
        subscription.Activate();
        subscription.Freeze(5);
        subscription.Activate();

        var result = subscription.Freeze(3);

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.CannotFreezeMoreThanAllowedFreezeCount.Code, result.TopError.Code);
    }

    [Fact]
    public void Freeze_ShouldReturnError_WhenFreezeDaysExceedPlanLimit()
    {
        var subscription = SubscriptionFactory.CreateSubscription(allowedFreezeCount: 2, maxTotalFreezeDays: 4).Value;
        subscription.Activate();

        var result = subscription.Freeze(5);

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.CannotFreezeMoreThanAllowedFreezeDays.Code, result.TopError.Code);
    }

    [Fact]
    public void Unfreeze_ShouldReturnSuccess_WhenFreezePeriodHasEnded()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();
        subscription.Freeze(0);

        var result = subscription.Unfreeze();

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    [Fact]
    public void Unfreeze_ShouldReturnError_WhenSubscriptionIsNotFrozen()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.Unfreeze();

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.OnlyFrozenSubscriptionsCanBeUnfrozen.Code, result.TopError.Code);
    }

    [Fact]
    public void Unfreeze_ShouldReturnError_WhenFreezePeriodHasNotEnded()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();
        subscription.Freeze(1);

        var result = subscription.Unfreeze();

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.FreezeEndDateNotNow.Code, result.TopError.Code);
    }

    [Fact]
    public void CanUnFreeze_ShouldReturnFalse_WhenSubscriptionIsNotFrozen()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.CanUnFreeze();

        Assert.False(result);
    }

    [Fact]
    public void CanUnFreeze_ShouldReturnFalse_WhenFreezePeriodHasNotEnded()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();
        subscription.Freeze(1);

        var result = subscription.CanUnFreeze();

        Assert.False(result);
    }

    [Fact]
    public void CanUnFreeze_ShouldReturnTrue_WhenFreezePeriodHasEnded()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();
        subscription.Freeze(0);

        var result = subscription.CanUnFreeze();

        Assert.True(result);
    }

    [Fact]
    public void Expire_ShouldReturnError_WhenSubscriptionIsNotActive()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.Expire();

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.OnlyActiveSubscriptionsCanBeExpired.Code, result.TopError.Code);
    }

    [Fact]
    public void Expire_ShouldReturnSuccess_WhenSubscriptionIsActive()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();

        var result = subscription.Expire();

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Expired, subscription.Status);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
    }
}