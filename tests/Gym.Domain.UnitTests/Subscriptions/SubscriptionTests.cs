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
    public void Create_ShouldReturnError_WhenStartDateIsInPast()
    {
        var result = SubscriptionFactory.CreateSubscription(startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.SubscriptionStartDateCannotBeInThePast.Code, result.TopError.Code);
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
    public void Scheduled_ShouldReturnSuccess_WhenStatusIsPending()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.Scheduled();

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Scheduled, subscription.Status);
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
    public void Expire_ShouldReturnError_WhenSubscriptionIsNotActive()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = subscription.Expire();

        Assert.False(result.IsSuccess);
        Assert.Equal(SubscriptionErrors.OnlyActiveSubscriptionsCanBeExpired.Code, result.TopError.Code);
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
