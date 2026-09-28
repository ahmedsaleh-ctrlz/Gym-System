using Gym.Application.Common.Interfaces;
using Gym.Application.Features.PromoCodes.Dtos;
using Gym.Application.PromoCodes;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.PromoCodes.Queries.GetPromoCodesQuery
{
    public sealed class GetPromoCodesQueryHandler(
    IAppDbContext context)
    : IRequestHandler<
        GetPromoCodesQuery,
        Result<IEnumerable<PromoCodeResponse>>>
    {
        public async Task<Result<IEnumerable<PromoCodeResponse>>> Handle(
            GetPromoCodesQuery request,
            CancellationToken ct)
        {
            var query = context.PromoCodes
                .AsNoTracking();

            if (request.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToUpper();

                query = query.Where(p => p.Code.Contains(search));
            }

            var result = await query
                .OrderByDescending(p => p.Id)
                .Select(PromoCodeMapper.ToResponse())
                .ToListAsync(ct);

            return result;
        }
    }
}
