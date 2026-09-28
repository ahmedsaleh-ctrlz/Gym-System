using System.Linq.Expressions;

using Gym.Application.Features.PromoCodes.Dtos;
using Gym.Domain.PromoCodes;

namespace Gym.Application.PromoCodes;

public static class PromoCodeMapper
{
    public static Expression<Func<PromoCode, PromoCodeResponse>> ToResponse()
    {
        return p => new PromoCodeResponse(
            p.Id,
            p.Code,
            p.PlanId,
            p.Plan.Title,
            p.DiscountType,
            p.DiscountValue,
            p.MaxDiscount,
            p.MinimumPurchaseAmount,
            p.Audience,
            p.UsageLimit,
            p.UsageCount,
            p.ExpiresAtUtc,
            p.IsActive);
    }

    public static PromoCodeResponse ToResponse(
        this PromoCode promoCode)
    {
        return new PromoCodeResponse(
            promoCode.Id,
            promoCode.Code,
            promoCode.PlanId,
            promoCode.Plan.Title,
            promoCode.DiscountType,
            promoCode.DiscountValue,
            promoCode.MaxDiscount ?? null,
            promoCode.MinimumPurchaseAmount ?? null,
            promoCode.Audience,
            promoCode.UsageLimit,
            promoCode.UsageCount,
            promoCode.ExpiresAtUtc,
            promoCode.IsActive);
    }
}