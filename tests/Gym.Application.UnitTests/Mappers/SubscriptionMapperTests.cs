using Gym.Application.Features.Subscriptions.Mappers;
using Gym.Tests.Common.Members;
using Gym.Tests.Common.Reflection;
using Gym.Tests.Common.Subscriptions;

namespace Gym.Application.UnitTests.Mappers;

public class SubscriptionMapperTests
{
    [Fact]
    public void ToDto_ShouldThrowNullReferenceException_WhenSubscriptionIsNull()
    {
        Assert.Throws<NullReferenceException>(() => SubscriptionMapper.ToDto(null!));
    }

    [Fact]
    public void ToDto_ShouldMapSubscriptionToResponse()
    {
        var member = MemberFactory.CreateMember(firstName: "Huda", lastName: "Maher").Value;
        ReflectionTestHelper.SetProperty(member, "Id", 12);

        var subscription = SubscriptionFactory.CreateSubscription(memberId: 12, planCost: 700m, durationInDays: 30).Value;
        ReflectionTestHelper.SetProperty(subscription, "Id", 22);
        ReflectionTestHelper.SetProperty(subscription, "Member", member);

        var result = subscription.ToDto();

        Assert.Equal(22, result.SubscriptionId);
        Assert.Equal(12, result.MemberId);
        Assert.Equal("Huda Maher", result.MemberName);
        Assert.Equal(subscription.Plan!.Title, result.PlanName);
        Assert.Equal(700m, result.PriceSnapshot);
        Assert.Equal(subscription.StartDate, result.StartDate);
        Assert.Equal(subscription.EndDate, result.EndDate);
        Assert.Equal(subscription.Status.ToString(), result.Status);
        Assert.Equal(subscription.FreezeCountUsed, result.FreezeCountUsed);
        Assert.Equal(subscription.TotalFreezeDaysUsed, result.TotalFreezeDaysUsed);
    }
}