using Gym.Application.Common.Interfaces;
using Gym.Application.Features.PromoCodes.Dtos;
using Gym.Application.PromoCodes;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.PromoCodes.Queries.GetPromoCodeByIdQuery
{
    public sealed class GetPromoCodeByIdQueryHandler(
    IAppDbContext context)
    : IRequestHandler<
        GetPromoCodeByIdQuery,
        Result<PromoCodeResponse>>
    {
        public async Task<Result<PromoCodeResponse>> Handle(
            GetPromoCodeByIdQuery request,
            CancellationToken ct)
        {
            var promoCode = await context.PromoCodes.Include(P => P.Plan)
                .AsNoTracking()
                .Where(p => p.Id == request.Id)
                .Select(PromoCodeMapper.ToResponse())
                .FirstOrDefaultAsync(ct);

            if (promoCode is null)
            {
                return Error.NotFound("PromoCode.NotFound", "Promo Code not found");
            }

            return promoCode;
        }
    }
}
