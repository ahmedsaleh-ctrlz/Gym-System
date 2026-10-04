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

    public static Error InvalidTransactionId => Error.Validation("Payment.InvalidTransactionId", "Transaction ID must be a non-empty string.");

    public static Error CannotRefundPayment => Error.Conflict("Payment.CannotRefundPayment", "Cannot Refund Payment after 7 days from pay time.");
    public static Error InvalidDiscount =>
        Error.Validation(
            "Payments.Validation",
            "Discount cannot be negative.");

    public static Error InvalidTax =>
        Error.Validation(
            "Payments.Validation",
            "Tax cannot be negative.");

    public static Error DiscountExceedsAmount =>
        Error.Validation(
            "Payments.Validation",
            "Discount cannot be greater than the payment amount.");

    public static Error PaymentReferenceRequired =>
    Error.Validation(
        "Payments.Validation",
        "Payment reference is required for this payment method.");

    public static Error PaymentReferenceNotAllowed =>
        Error.Validation(
            "Payments.Validation",
            "Payment reference is not allowed for this payment method.");

    public static Error PaymentAmountMustBeGreaterThanZero => Error.Conflict("Payment.Conflict", "Payment Amount Must Be Greater Than Zero");
    public static Error PaymentAmountMustBeZero => Error.Conflict("Payment.Conflict", "Payment Amount Must Be Zero to Complete Free Payment");

    public static Error PromoCodeNotApplied => Error.Conflict("Payment.Conflict", "Promo Code Not Applied");

    public static Error OnlyUnderReviewPaymentsCanBeRejected =>
    Error.Validation(
        "Payments.Validation",
        "Only payments under review can be rejected.");
    public static Error InvalidPaymentMethodForReview =>
        Error.Validation("Payment.InvalidSubscription", "Payment must be linked to a valid subscription.");
}