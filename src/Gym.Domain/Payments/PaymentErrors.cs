using Gym.Domain.Common.Result;

namespace Gym.Domain.Payments;

public static class PaymentErrors
{
    public static Error InvalidSubscription =>
        Error.Validation("Payment.InvalidSubscription", "Payment must be linked to a valid subscription.");

    public static Error InvalidSubscriptionId =>
        Error.Validation("Payment.InvalidSubscriptionId", "Payment must be linked to a valid subscription.");

    public static Error InvalidAmount =>
        Error.Validation("Payment.InvalidAmount", "Payment amount must be greater than 0.");

    public static Error PaymentCanOnlyBeRecordedForPendingSubscription =>
        Error.Conflict("Payment.SubscriptionNotPending", "Payments can only be recorded for pending subscriptions.");
    
    public static Error PaymentAlreadyPaid => Error.Conflict("Payment.AlreadyPaid", "Payment has already been made.");

    public static Error InvalidPaymentStatus => Error.Conflict("Payment.InvalidPaymentStatus", "Invalid payment status.");
}
