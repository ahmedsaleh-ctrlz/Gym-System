using Gym.Domain.Common.Result;

namespace Gym.Domain.PromoCodes;

public static class PromoCodeUsageErrors
{
    public static Error InvalidPromoCode =>
        Error.Validation(
            "PromoCodeUsages.Validation",
            "Invalid promo code.");

    public static Error InvalidMember =>
        Error.Validation(
            "PromoCodeUsages.Validation",
            "Invalid member.");
}