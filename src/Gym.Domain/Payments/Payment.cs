using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Subscriptions;
using Gym.Domain.Subscriptions.Enums;

namespace Gym.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    public int SubscriptionId { get; private set; }
    public Subscription Subscription { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; } = null;
    public PaymentStatus Status { get; private set; }
    public DateTime? PaidAtUtc { get; private set; } = null;

    private Payment()
    {
    }

    private Payment(Subscription subscription)
    {
        Subscription = subscription;
        Amount = subscription.PriceSnapshot;
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

    public Result<Updated> Pay(PaymentMethod paymentMethod)
    {
        if (Status != PaymentStatus.Pending)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;

        return Result.Updated;
    }

    private static Error? Validate(Subscription subscription)
    {
        if (subscription is null)
        {
            return PaymentErrors.InvalidSubscription;
        }

        if (subscription.Status != SubscriptionStatus.Pending)
        {
            return PaymentErrors.PaymentCanOnlyBeRecordedForPendingSubscription;
        }

        if (subscription.PriceSnapshot <= 0)
        {
            return PaymentErrors.InvalidAmount;
        }

        return null;
    }
}