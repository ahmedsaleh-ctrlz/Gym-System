using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.PromoCodes.Dtos;
using Gym.Application.PromoCodes;
using Gym.Domain.Common.Result;
using Gym.Domain.PromoCodes.Enums;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.PromoCodes.Queries.GetByCode
{
    public sealed record GetPromoCodeByCodeQuery(
    string Code, int MemberId, int PlanId)
    : IRequest<Result<PromoCodeResponse>>;

    public class GetPromoCodeByCodeQueryHandler(IAppDbContext context) : IRequestHandler<GetPromoCodeByCodeQuery, Result<PromoCodeResponse>>
    {
        public async Task<Result<PromoCodeResponse>> Handle(GetPromoCodeByCodeQuery request, CancellationToken cancellationToken)
        {
            var promoCode = await context.PromoCodes.Include(p => p.Plan)
                .AsNoTracking()
                .Where(p => p.Code == request.Code && p.ExpiresAtUtc > DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            if (promoCode is null)
            {
                return ApplicationErrors.PromoCodeNotValid;
            }

            var member = await context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == request.MemberId);

            if(member is null)
            {
                return ApplicationErrors.MemberNotFound;
            }

            if(promoCode.PlanId != request.PlanId)
            {
                return ApplicationErrors.PromoCodeNotApplicableToPlan;
            }

            if(promoCode.Audience == PromoCodeAudience.NewMembers)
            {
                if (await context.Subscriptions.AnyAsync(s => s.MemberId == request.MemberId && s.Status != SubscriptionStatus.Cancelled))
                {
                    return ApplicationErrors.PromoCodeOnlyForNewMembers;
                }
            }

            return promoCode.ToResponse();
        }
    }
}
