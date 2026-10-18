using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.PromoCodes.Enums;
using Gym.Domain.Subscriptions.Enums;
using Gym.Tests.Common.Payments;
using Gym.Tests.Common.PromoCodes;
using Gym.Tests.Common.Reflection;
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
        subscription.Scheduled();

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
        Assert.Equal(subscription.Price, result.Value.Amount);
        Assert.Equal(subscription.Price, result.Value.SubTotal);
        Assert.Equal(0m, result.Value.Discount);
        Assert.Equal(0m, result.Value.Tax);
        Assert.Equal(PaymentStatus.Pending, result.Value.Status);
        Assert.Null(result.Value.PaymentMethod);
        Assert.Null(result.Value.PaidAtUtc);
    }

    [Fact]
    public void Pay_ShouldReturnError_WhenPaymentIsAlreadyPaid()
    {
        var payment = PaymentFactory.CreatePayment(payImmediately: true).Value;

        var result = payment.Pay(PaymentMethod.Cash);

        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentErrors.InvalidPaymentStatus.Code, result.TopError.Code);
    }

    [Fact]
    public void Pay_ShouldReturnSuccess_AndSetPaymentValues_WhenMethodIsCash()
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

    [Theory]
    [InlineData(PaymentMethod.EWallet)]
    [InlineData(PaymentMethod.InstaPay)]
    public void Pay_ShouldReturnError_WhenPaymentReferenceRequiredButMissing(PaymentMethod method)
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.Pay(method, null, "  ");

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.PaymentReferenceRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CompleteFreePayment_ShouldReturnError_WhenStatusIsNotPending()
    {
        var payment = PaymentFactory.CreatePayment(payImmediately: true).Value;

        var result = payment.CompleteFreePayment();

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.InvalidPaymentStatus.Code, result.TopError.Code);
    }

    [Fact]
    public void CompleteFreePayment_ShouldReturnError_WhenAmountIsNotZero()
    {
        var payment = PaymentFactory.CreatePayment().Value; // Amount is default 500

        var result = payment.CompleteFreePayment();

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.PaymentAmountMustBeZero.Code, result.TopError.Code);
    }

    [Fact]
    public void CompleteFreePayment_ShouldReturnSuccess_WhenAmountIsZeroVia100PercentPromo()
    {
        var payment = PaymentFactory.CreatePayment().Value;
        var freePromo = PromoCodeFactory.CreatePromoCode(
            planId: payment.Subscription.PlanId,
            discountType: DiscountType.Percentage,
            discountValue: 100m,
            maxDiscount: null,
            minimumPurchaseAmount: 0m).Value;

        var applyResult = payment.ApplyPromoCode(freePromo);
        Assert.True(applyResult.IsSuccess);
        Assert.Equal(0m, payment.Amount);

        var result = payment.CompleteFreePayment();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.NotNull(payment.PaidAtUtc);
    }

    [Fact]
    public void SubmitForReview_ShouldReturnError_WhenStatusIsNotPending()
    {
        var payment = PaymentFactory.CreatePayment(payImmediately: true).Value;

        var result = payment.SubmitForReview(PaymentMethod.InstaPay, null, "REF-123");

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.InvalidPaymentStatus.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Visa)]
    public void SubmitForReview_ShouldReturnError_WhenPaymentMethodIsNotEWalletOrInstaPay(PaymentMethod invalidMethod)
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.SubmitForReview(invalidMethod, null, "REF-123");

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.InvalidPaymentMethodForReview.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SubmitForReview_ShouldReturnError_WhenPaymentReferenceIsMissing(string? invalidRef)
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.SubmitForReview(PaymentMethod.EWallet, null, invalidRef!);

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.PaymentReferenceRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void SubmitForReview_ShouldReturnError_WhenAmountIsZeroOrNegative()
    {
        var payment = PaymentFactory.CreatePayment().Value;
        var freePromo = PromoCodeFactory.CreatePromoCode(
            planId: payment.Subscription.PlanId,
            discountType: DiscountType.Percentage,
            discountValue: 100m,
            maxDiscount: null,
            minimumPurchaseAmount: 0m).Value;
        payment.ApplyPromoCode(freePromo);

        var result = payment.SubmitForReview(PaymentMethod.EWallet, null, "REF-123");

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.PaymentAmountMustBeGreaterThanZero.Code, result.TopError.Code);
    }

    [Fact]
    public void SubmitForReview_ShouldReturnSuccess_AndSetStatusToUnderReview()
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.SubmitForReview(PaymentMethod.InstaPay, null, "INSTA-9999");

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.UnderReview, payment.Status);
        Assert.Equal(PaymentMethod.InstaPay, payment.PaymentMethod);
        Assert.Equal("INSTA-9999", payment.PaymentReference);
    }

    [Fact]
    public void Approve_ShouldReturnError_WhenStatusIsNotUnderReview()
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.Approve();

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.InvalidPaymentStatus.Code, result.TopError.Code);
    }

    [Fact]
    public void Approve_ShouldSetPaidStatusAndTimestamp_WhenUnderReview()
    {
        var payment = PaymentFactory.CreatePayment().Value;
        payment.SubmitForReview(PaymentMethod.InstaPay, null, "REF-123");
        var beforeApprove = DateTime.UtcNow;

        var result = payment.Approve();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.NotNull(payment.PaidAtUtc);
        Assert.True(payment.PaidAtUtc >= beforeApprove.AddSeconds(-1));
    }

    [Fact]
    public void Reject_ShouldReturnError_WhenStatusIsNotUnderReview()
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.Reject();

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.OnlyUnderReviewPaymentsCanBeRejected.Code, result.TopError.Code);
    }

    [Fact]
    public void Reject_ShouldCancelPaymentAndSubscription_WhenUnderReview()
    {
        var payment = PaymentFactory.CreatePayment().Value;
        payment.SubmitForReview(PaymentMethod.EWallet, null, "REF-WALLET");

        var result = payment.Reject();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Cancelled, payment.Status);
        Assert.Equal(SubscriptionStatus.Cancelled, payment.Subscription.Status);
    }

    [Fact]
    public void Cancel_ShouldReturnError_WhenStatusIsNotPending()
    {
        var payment = PaymentFactory.CreatePayment(payImmediately: true).Value;

        var result = payment.Cancel();

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.InvalidPaymentStatus.Code, result.TopError.Code);
    }

    [Fact]
    public void Cancel_ShouldCancelPaymentAndSubscription_WhenPending()
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Cancelled, payment.Status);
        Assert.Equal(SubscriptionStatus.Cancelled, payment.Subscription.Status);
    }

    [Fact]
    public void Refund_ShouldReturnError_WhenStatusIsNotPaid()
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.Refund();

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.InvalidPaymentStatus.Code, result.TopError.Code);
    }

    [Fact]
    public void Refund_ShouldReturnError_WhenPaymentWasPaidMoreThan7DaysAgo()
    {
        var payment = PaymentFactory.CreatePayment(payImmediately: true).Value;
        ReflectionTestHelper.SetProperty(payment, nameof(Payment.PaidAtUtc), DateTime.UtcNow.AddDays(-7).AddMinutes(-5));

        var result = payment.Refund();

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.CannotRefundPayment.Code, result.TopError.Code);
    }

    [Fact]
    public void Refund_ShouldReturnSuccess_WhenWithin7DaysBoundary()
    {
        var payment = PaymentFactory.CreatePayment(payImmediately: true).Value;

        // Paid 6 days ago (well within 7 days)
        ReflectionTestHelper.SetProperty(payment, nameof(Payment.PaidAtUtc), DateTime.UtcNow.AddDays(-6));

        var result = payment.Refund();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(SubscriptionStatus.Cancelled, payment.Subscription.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetExternalTransactionId_ShouldReturnError_WhenIdIsInvalid(string? invalidId)
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.SetExternalTransactionId(invalidId!);

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.InvalidTransactionId.Code, result.TopError.Code);
    }

    [Fact]
    public void SetExternalTransactionId_ShouldReturnSuccess_AndSetField()
    {
        var payment = PaymentFactory.CreatePayment().Value;

        var result = payment.SetExternalTransactionId("pi_stripe_12345");

        Assert.True(result.IsSuccess);
        Assert.Equal("pi_stripe_12345", payment.ExternalTransactionId);
    }

    [Fact]
    public void ApplyPromoCode_ShouldReturnError_WhenPaymentIsNotPending()
    {
        var payment = PaymentFactory.CreatePayment(payImmediately: true).Value;
        var promo = PromoCodeFactory.CreatePromoCode(planId: payment.Subscription.PlanId).Value;

        var result = payment.ApplyPromoCode(promo);

        Assert.True(result.IsError);
        Assert.Equal(PaymentErrors.InvalidPaymentStatus.Code, result.TopError.Code);
    }

    [Fact]
    public void ApplyPromoCode_ShouldApplyDiscountAndRecalculateAmount()
    {
        var payment = PaymentFactory.CreatePayment().Value; // SubTotal = 500
        var promo = PromoCodeFactory.CreatePromoCode(
            planId: payment.Subscription.PlanId,
            discountType: DiscountType.FixedAmount,
            discountValue: 100m,
            maxDiscount: null,
            minimumPurchaseAmount: 200m).Value;
        ReflectionTestHelper.SetProperty(promo, nameof(promo.Id), 42);

        var result = payment.ApplyPromoCode(promo);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, payment.PromoCodeId);
        Assert.Equal(100m, payment.Discount);
        Assert.Equal(400m, payment.Amount);
    }
}