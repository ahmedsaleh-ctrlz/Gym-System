using Gym.Domain.Common.Result;

namespace Gym.Domain.PromoCodes;

public static class PromoCodeErrors
{
    public static Error CodeRequired =>
        Error.Validation(
            "PromoCodes.Validation",
            "Promo code is required.");

    public static Error InvalidPlan =>
        Error.Validation(
            "PromoCodes.Validation",
            "Invalid plan.");

    public static Error InvalidDiscountValue =>
        Error.Validation(
            "PromoCodes.Validation",
            "Discount value must be greater than zero.");

    public static Error InvalidPercentage =>
        Error.Validation(
            "PromoCodes.Validation",
            "Percentage discount cannot exceed 100%.");

    public static Error InvalidMaxDiscount =>
        Error.Validation(
            "PromoCodes.Validation",
            "Maximum discount must be greater than zero.");

    public static Error MaxDiscountNotAllowedForFixedAmount =>
        Error.Validation(
            "PromoCodes.Validation",
            "Maximum discount is only allowed for percentage discounts.");

    public static Error InvalidMinimumPurchaseAmount =>
        Error.Validation(
            "PromoCodes.Validation",
            "Minimum purchase amount cannot be negative.");

    public static Error InvalidUsageLimit =>
        Error.Validation(
            "PromoCodes.Validation",
            "Usage limit must be greater than zero.");

    public static Error InvalidExpirationDate =>
        Error.Validation(
            "PromoCodes.Validation",
            "Expiration date must be in the future.");

    public static Error AlreadyActive =>
        Error.Conflict(
            "PromoCodes.Conflict",
            "Promo code is already active.");

    public static Error AlreadyInactive =>
        Error.Conflict(
            "PromoCodes.Conflict",
            "Promo code is already inactive.");

    public static Error UsageLimitReached =>
        Error.Conflict(
            "PromoCodes.Conflict",
            "Promo code usage limit has been reached.");

    public static Error NotActive =>
    Error.Conflict(
        "PromoCodes.NotActive",
        "Promo code is not active.");

    public static Error Expired =>
        Error.Validation(
            "PromoCodes.Expired",
            "Promo code has expired.");

    public static Error NotApplicableToPlan =>
        Error.Validation(
            "PromoCodes.NotApplicableToPlan",
            "Promo code is not applicable to this plan.");

    public static Error MinimumPurchaseAmountNotMet =>
        Error.Validation(
            "PromoCodes.MinimumPurchaseAmountNotMet",
            "The minimum purchase amount for this promo code has not been met.");
}