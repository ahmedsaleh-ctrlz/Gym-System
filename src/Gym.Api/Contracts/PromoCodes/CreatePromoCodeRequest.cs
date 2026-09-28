using Gym.Domain.PromoCodes.Enums;

namespace Gym.Api.Contracts.PromoCodes
{
    public sealed record CreatePromoCodeRequest(string Code,
                                                int PlanId,
                                                DiscountType DiscountType,
                                                decimal DiscountValue,
                                                decimal? MaxDiscount,
                                                decimal? MinimumPurchaseAmount,
                                                PromoCodeAudience Audience,
                                                int UsageLimit,
                                                DateTime ExpiresAtUtc);
}
