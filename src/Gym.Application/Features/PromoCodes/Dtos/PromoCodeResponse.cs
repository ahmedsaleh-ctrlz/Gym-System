using Gym.Domain.PromoCodes.Enums;

namespace Gym.Application.Features.PromoCodes.Dtos
{
    public sealed record PromoCodeResponse(
    int Id,
    string Code,
    int PlanId,
    string PlanName,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscount,
    decimal? MinimumPurchaseAmount,
    PromoCodeAudience Audience,
    int UsageLimit,
    int UsageCount,
    DateTime ExpiresAtUtc,
    bool IsActive);
}
