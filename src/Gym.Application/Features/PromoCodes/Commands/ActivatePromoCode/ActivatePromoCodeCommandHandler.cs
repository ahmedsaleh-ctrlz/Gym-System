using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Gym.Application.Features.PromoCodes.Commands.ActivatePromoCode
{
    public sealed class ActivatePromoCodeCommandHandler(
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<ActivatePromoCodeCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(
            ActivatePromoCodeCommand request,
            CancellationToken ct)
        {
            var promoCode = await context.PromoCodes
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (promoCode is null)
            {
                return Error.NotFound("PromoCode.NotFound", "Promo Code Not Found");
            }

            var result = promoCode.Activate();

            if (result.IsError)
            {
                return result.TopError;
            }

            await context.SaveChangesAsync(ct);

            await cache.RemoveByTagAsync("PromoCodes", ct);

            return Result.Updated;
        }
    }
}
