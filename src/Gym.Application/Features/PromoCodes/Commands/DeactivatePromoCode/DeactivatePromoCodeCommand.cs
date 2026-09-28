using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.PromoCodes;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Gym.Application.Features.PromoCodes.Commands.DeactivatePromoCode
{
    public sealed record DeactivatePromoCodeCommand(
    int Id)
    : IRequest<Result<Updated>>;

    public sealed class DeactivatePromoCodeCommandHandler(
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<DeactivatePromoCodeCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(
            DeactivatePromoCodeCommand request,
            CancellationToken ct)
        {
            var promoCode = await context.PromoCodes
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (promoCode is null)
            {
                return Error.NotFound("PromoCode.NotFound", "Promo Code Not Found");
            }

            var result = promoCode.Deactivate();

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
