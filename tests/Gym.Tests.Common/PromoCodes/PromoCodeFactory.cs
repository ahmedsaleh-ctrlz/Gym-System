using Gym.Domain.Common.Result;
using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.Enums;
using Gym.Domain.PromoCodes.PromoCodeUsage;

namespace Gym.Tests.Common.PromoCodes;

public static class PromoCodeFactory
{
    public static Result<PromoCode> CreatePromoCode(
        string code = "DISCOUNT20",
        int planId = 1,
        DiscountType discountType = DiscountType.Percentage,
        decimal discountValue = 20m,
        decimal? maxDiscount = 50m,
        decimal? minimumPurchaseAmount = 100m,
        PromoCodeAudience audience = PromoCodeAudience.Everyone,
        int usageLimit = 10,
        DateTime? expiresAtUtc = null)
    {
        return PromoCode.Create(
            code,
            planId,
            discountType,
            discountValue,
            maxDiscount,
            minimumPurchaseAmount,
            audience,
            usageLimit,
            expiresAtUtc ?? DateTime.UtcNow.AddDays(30));
    }

    public static Result<PromoCodeUsage> CreatePromoCodeUsage(
        int promoCodeId = 1,
        int memberId = 1)
    {
        return PromoCodeUsage.Create(promoCodeId, memberId);
    }
}
