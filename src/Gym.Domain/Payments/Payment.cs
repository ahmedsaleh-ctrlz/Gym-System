using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;
using Gym.Domain.PromoCodes;
using Gym.Domain.Subscriptions;

namespace Gym.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    public Subscription Subscription { get; private set; } = null!;
    public PromoCode? PromoCode { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTime PaidAt { get; private set; }
    public decimal Tax { get; private set; }
    public PaymentStatus Status { get; private set; }

    private Payment()
    {
    }

    private Payment(
        Subscription subscription,
        PromoCode? promoCode,
        decimal amount,
        PaymentMethod method,
        DateTime paidAt,
        decimal tax,
        PaymentStatus status)
    {
        Subscription = subscription;
        PromoCode = promoCode;
        Amount = amount;
        Method = method;
        PaidAt = paidAt;
        Tax = tax;
        Status = status;
    }

    public static Result<Payment> Create(
        Subscription subscription,
        PromoCode? promoCode,
        decimal amount,
        PaymentMethod method,
        DateTime paidAt,
        decimal tax,
        PaymentStatus status)
    {
        var error = Validate(subscription, amount, method, paidAt, tax, status);
        if (error is not null)
        {
            return error;
        }

        if (promoCode is not null)
        {
            var promoCodeResult = promoCode.RegisterUsage(paidAt);
            if (promoCodeResult.IsError)
            {
                return promoCodeResult.TopError;
            }
        }

        return new Payment(subscription, promoCode, amount, method, paidAt, tax, status);
    }

    public Result<Updated> UpdateInfo(
        PromoCode? promoCode,
        decimal amount,
        PaymentMethod method,
        DateTime paidAt,
        decimal tax,
        PaymentStatus status)
    {
        var error = Validate(Subscription, amount, method, paidAt, tax, status);
        if (error is not null)
        {
            return error;
        }

        if (promoCode is not null && !ReferenceEquals(PromoCode, promoCode))
        {
            var promoCodeResult = promoCode.RegisterUsage(paidAt);
            if (promoCodeResult.IsError)
            {
                return promoCodeResult.TopError;
            }
        }

        PromoCode = promoCode;
        Amount = amount;
        Method = method;
        PaidAt = paidAt;
        Tax = tax;
        Status = status;

        return Result.Updated;
    }

    public Result<Updated> ChangeStatus(PaymentStatus status)
    {
        if (status is PaymentStatus.Unknown)
        {
            return PaymentError.InvalidStatus;
        }

        Status = status;
        return Result.Updated;
    }

    private static Error? Validate(
        Subscription subscription,
        decimal amount,
        PaymentMethod method,
        DateTime paidAt,
        decimal tax,
        PaymentStatus status)
    {
        if (subscription is null)
        {
            return PaymentError.SubscriptionRequired;
        }

        if (!subscription.CanAcceptPayments)
        {
            return PaymentError.SubscriptionCannotAcceptPayments;
        }

        if (amount < 0)
        {
            return PaymentError.InvalidAmount;
        }

        if (method is PaymentMethod.Unknown)
        {
            return PaymentError.InvalidMethod;
        }

        if (paidAt > DateTime.UtcNow)
        {
            return PaymentError.InvalidPaidAt;
        }

        if (tax < 0)
        {
            return PaymentError.InvalidTax;
        }

        if (status is PaymentStatus.Unknown)
        {
            return PaymentError.InvalidStatus;
        }

        return null;
    }
}
