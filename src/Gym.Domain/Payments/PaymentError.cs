using Gym.Domain.Common.Result;

namespace Gym.Domain.Payments;

public static class PaymentError
{
    public static Error SubscriptionRequired => Error.Validation("Payment_Subscription_Required", "PaymentSubscriptionRequired");
    public static Error SubscriptionCannotAcceptPayments => Error.Conflict("Payment_Subscription_Cannot_Accept_Payments", "PaymentSubscriptionCannotAcceptPayments");
    public static Error InvalidAmount => Error.Validation("Payment_Amount_Invalid", "PaymentAmountInvalid");
    public static Error InvalidMethod => Error.Validation("Payment_Method_Invalid", "PaymentMethodInvalid");
    public static Error InvalidPaidAt => Error.Validation("Payment_Paid_At_Invalid", "PaymentPaidAtInvalid");
    public static Error InvalidTax => Error.Validation("Payment_Tax_Invalid", "PaymentTaxInvalid");
    public static Error InvalidStatus => Error.Validation("Payment_Status_Invalid", "PaymentStatusInvalid");
}
