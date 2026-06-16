using Gym.Application.Features.Payments.Mappers;
using Gym.Domain.Payments.Enums;
using Gym.Tests.Common.Members;
using Gym.Tests.Common.Payments;
using Gym.Tests.Common.Reflection;
using Gym.Tests.Common.Subscriptions;

namespace Gym.Application.UnitTests.Mappers;

public class PaymentMapperTests
{
    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenPaymentIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => PaymentMapper.ToDto(null!));
    }

    [Fact]
    public void ToDto_ShouldMapPaymentToResponse()
    {
        var member = MemberFactory.CreateMember(firstName: "Lina", lastName: "Adel").Value;
        ReflectionTestHelper.SetProperty(member, "Id", 5);

        var subscription = SubscriptionFactory.CreateSubscription(memberId: 5).Value;
        ReflectionTestHelper.SetProperty(subscription, "Id", 13);
        ReflectionTestHelper.SetProperty(subscription, "Member", member);

        var payment = PaymentFactory.CreatePayment(subscription).Value;
        ReflectionTestHelper.SetProperty(payment, "Id", 17);
        ReflectionTestHelper.SetProperty(payment, "SubscriptionId", 13);
        payment.Pay(PaymentMethod.Visa);

        var result = payment.ToDto();

        Assert.Equal(17, result.PaymentId);
        Assert.Equal(13, result.SubscriptionId);
        Assert.Equal(5, result.MemberId);
        Assert.Equal("Lina Adel", result.MemberName);
        Assert.Equal(subscription.Plan!.Title, result.PlanName);
        Assert.Equal(payment.Amount, result.Amount);
        Assert.Equal(PaymentMethod.Visa.ToString(), result.PaymentMethod);
        Assert.Equal(payment.Status.ToString(), result.Status);
        Assert.Equal(payment.PaidAtUtc, result.PaidAtUtc);
    }

    [Fact]
    public void ToDto_ShouldMapNullPaymentMethodToEmptyString()
    {
        var member = MemberFactory.CreateMember(firstName: "Lina", lastName: "Adel").Value;
        ReflectionTestHelper.SetProperty(member, "Id", 5);

        var subscription = SubscriptionFactory.CreateSubscription(memberId: 5).Value;
        ReflectionTestHelper.SetProperty(subscription, "Member", member);

        var payment = PaymentFactory.CreatePayment(subscription).Value;

        var result = payment.ToDto();

        Assert.Equal(string.Empty, result.PaymentMethod);
        Assert.Equal("Pending", result.Status);
    }
}