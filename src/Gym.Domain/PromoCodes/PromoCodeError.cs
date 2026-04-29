using Gym.Domain.Common.Result;

namespace Gym.Domain.PromoCodes;

public static class PromoCodeError
{
    public static Error CodeRequired => Error.Validation("Promo_Code_Required", "PromoCodeRequired");
    public static Error InvalidDiscountAmount => Error.Validation("Promo_Code_Discount_Invalid", "PromoCodeDiscountInvalid");
    public static Error InvalidUsedQuantity => Error.Validation("Promo_Code_Used_Quantity_Invalid", "PromoCodeUsedQuantityInvalid");
    public static Error InvalidMaxQuantity => Error.Validation("Promo_Code_Max_Quantity_Invalid", "PromoCodeMaxQuantityInvalid");
    public static Error InvalidDuration => Error.Validation("Promo_Code_Duration_Invalid", "PromoCodeDurationInvalid");
    public static Error InvalidType => Error.Validation("Promo_Code_Type_Invalid", "PromoCodeTypeInvalid");
    public static Error PromoCodeInactive => Error.Conflict("Promo_Code_Inactive", "PromoCodeInactive");
    public static Error PromoCodeUsageExceeded => Error.Conflict("Promo_Code_Usage_Exceeded", "PromoCodeUsageExceeded");
}
