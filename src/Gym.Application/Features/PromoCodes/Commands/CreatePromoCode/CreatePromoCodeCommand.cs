using Gym.Domain.Common.Result;
using Gym.Domain.PromoCodes.Enums;

using MediatR;

namespace Gym.Application.Features.PromoCodes.Commands.CreatePromoCodeCommand
{
    public sealed record CreatePromoCodeCommand(
    string Code,
    int PlanId,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscount,
    decimal? MinimumPurchaseAmount,
    PromoCodeAudience Audience,
    int UsageLimit,
    DateTime ExpiresAtUtc)
    : IRequest<Result<Created>>;
}
