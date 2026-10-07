using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;
using Gym.Domain.PromoCodes;
using Gym.Domain.Subscriptions;
using Gym.Domain.Subscriptions.Enums;

namespace Gym.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    public int SubscriptionId { get; private set; }
    public Subscription Subscription { get; private set; } = null!;

    public int? PromoCodeId { get; private set; }
    public PromoCode? PromoCode { get; private set; }

    public decimal SubTotal { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Amount { get; private set; }

    public PaymentMethod? PaymentMethod { get; private set; }
    public string? PaymentReference { get; private set; }
    public string? ExternalTransactionId { get; private set; }

    public PaymentStatus Status { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    private Payment()
    {
    }

    private Payment(Subscription subscription)
    {
        Subscription = subscription;

        SubTotal = subscription.Price;
        Discount = 0;
        Tax = 0;
        Amount = SubTotal;

        Status = PaymentStatus.Pending;
    }

    public static Result<Payment> Create(
        Subscription subscription)
    {
        var error = Validate(subscription);

        if (error is not null)
        {
            return error;
        }

        return new Payment(subscription);
    }

    public Result<Updated> Pay(
        PaymentMethod paymentMethod,
        PromoCode? promoCode = null,
        string? paymentReference = null)
    {
        if (Status != PaymentStatus.Pending)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        if (promoCode is not null)
        {
            ApplyPromoCode(promoCode);
        }

        if (RequiresPaymentReference(paymentMethod) &&
            string.IsNullOrWhiteSpace(paymentReference))
        {
            return PaymentErrors.PaymentReferenceRequired;
        }

        PaymentMethod = paymentMethod;
        PaymentReference = paymentReference;

        Status = PaymentStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;

        return Result.Updated;
    }

    public Result<Updated> CompleteFreePayment(
        PromoCode? promoCode = null)
    {
        if (Status != PaymentStatus.Pending)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        if (promoCode is not null)
        {
            ApplyPromoCode(promoCode);
        }

        if (Amount != 0)
        {
            return PaymentErrors.PaymentAmountMustBeZero;
        }

        Status = PaymentStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;

        return Result.Updated;
    }

    public Result<Updated> SubmitForReview(
    PaymentMethod paymentMethod,
    PromoCode? promoCode,
    string paymentReference)
{
    if (Status != PaymentStatus.Pending)
    {
        return PaymentErrors.InvalidPaymentStatus;
    }

    if (paymentMethod is not
        (Enums.PaymentMethod.EWallet or Enums.PaymentMethod.InstaPay))
    {
        return PaymentErrors.InvalidPaymentMethodForReview;
    }

    if (string.IsNullOrWhiteSpace(paymentReference))
    {
        return PaymentErrors.PaymentReferenceRequired;
    }

    if (promoCode is not null)
    {
        ApplyPromoCode(promoCode);
    }

    if (Amount <= 0)
    {
        return PaymentErrors.PaymentAmountMustBeGreaterThanZero;
    }

    PaymentMethod = paymentMethod;
    PaymentReference = paymentReference;

    Status = PaymentStatus.UnderReview;

    return Result.Updated;
}

    public Result<Updated> Approve()
    {
        if (Status != PaymentStatus.UnderReview)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        Status = PaymentStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;

        return Result.Updated;
    }

    public Result<Updated> Reject()
    {
        if (Status != PaymentStatus.UnderReview)
        {
            return PaymentErrors.OnlyUnderReviewPaymentsCanBeRejected;
        }

        Status = PaymentStatus.Cancelled;
        Subscription.Cancel();

        return Result.Updated;
    }

    public Result<Updated> Cancel()
    {
        if (Status != PaymentStatus.Pending)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        Status = PaymentStatus.Cancelled;
        Subscription.Cancel();

        return Result.Updated;
    }

    public Result<Updated> Refund()
    {
        if (Status != PaymentStatus.Paid)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        if (PaidAtUtc is null ||
            PaidAtUtc.Value.AddDays(7) < DateTime.UtcNow)
        {
            return PaymentErrors.CannotRefundPayment;
        }

        Status = PaymentStatus.Refunded;

        Subscription.Cancel();

        return Result.Updated;
    }

    public Result<Updated> SetExternalTransactionId(
        string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return PaymentErrors.InvalidTransactionId;
        }

        ExternalTransactionId = transactionId;

        return Result.Updated;
    }

    public Result<Updated> ApplyPromoCode(PromoCode promoCode)
    {
        if (Status != PaymentStatus.Pending)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        var appliedResultError = promoCode.CanBeAppliedTo(Subscription.PlanId, SubTotal);

        if (appliedResultError is not null)
        {
            return appliedResultError;
        }

        var discount = promoCode.CalculateDiscount(SubTotal);

        PromoCodeId = promoCode.Id;
        PromoCode = promoCode;
        Discount = discount;

        RecalculateAmount();

        return Result.Updated;
    }

    private void RecalculateAmount()
    {
        var discountedAmount = SubTotal - Discount;

        if (discountedAmount <= 0)
        {
            Discount = SubTotal;
            Tax = 0;
            Amount = 0;
            return;
        }

        Amount = discountedAmount + Tax;
    }

    private static bool RequiresPaymentReference(
        PaymentMethod paymentMethod)
    {
        return paymentMethod is
            Enums.PaymentMethod.EWallet or
            Enums.PaymentMethod.InstaPay;
    }

    private static Error? Validate(
        Subscription subscription)
    {
        if (subscription is null)
        {
            return PaymentErrors.InvalidSubscription;
        }

        if (subscription.Status != SubscriptionStatus.Pending)
        {
            return PaymentErrors.PaymentCanOnlyBeRecordedForPendingSubscription;
        }

        if (subscription.Price <= 0)
        {
            return PaymentErrors.InvalidAmount;
        }

        return null;
    }
}