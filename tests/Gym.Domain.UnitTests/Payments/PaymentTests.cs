using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Subscriptions;
using Gym.Tests.Common.Payments;
using Gym.Tests.Common.Subscriptions;

namespace Gym.Domain.UnitTests.Payments;

public class PaymentTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenSubscriptionIsNull()
    {
        var result = Payment.Create(null!);

        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentErrors.InvalidSubscription.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenSubscriptionIsNotPending()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;
        subscription.Activate();

        var result = Payment.Create(subscription);

        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentErrors.PaymentCanOnlyBeRecordedForPendingSubscription.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenPriceSnapshotIsInvalid()
    {
        var subscriptionResult = SubscriptionFactory.CreateSubscription(planCost: 0m);

        Assert.True(subscriptionResult.IsSuccess);

        var result = Payment.Create(subscriptionResult.Value);

        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentErrors.InvalidAmount.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenSubscriptionIsPending()
    {
        var subscription = SubscriptionFactory.CreateSubscription().Value;

        var result = Payment.Create(subscription);

        Assert.True(result.IsSuccess);
        Assert.Equal(subscription, result.Value.Subscription);
        Assert.Equal(subscription.PriceSnapshot, result.Value.Amount);
        Assert.Equal(PaymentStatus.Pending, result.Value.Status);
        Assert.Null(result.Value.PaymentMethod);
        Assert.Null(result.Value.PaidAtUtc);
    }

    [Fact]
    public void Pay_ShouldReturnError_WhenPaymentStatusIsNotPending()
    {
        var payment = PaymentFactory.CreatePayment(payImmediately: true).Value;

        var result = payment.Pay(PaymentMethod.Visa);

        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentErrors.InvalidPaymentStatus.Code, result.TopError.Code);
    }

    [Fact]
    public void Pay_ShouldReturnSuccess_AndSetPaymentValues()
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var beforePay = DateTime.UtcNow;
        var result = payment.Pay(PaymentMethod.Cash);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentMethod.Cash, payment.PaymentMethod);
        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.NotNull(payment.PaidAtUtc);
        Assert.True(payment.PaidAtUtc >= beforePay.AddSeconds(-1));
    }
}
