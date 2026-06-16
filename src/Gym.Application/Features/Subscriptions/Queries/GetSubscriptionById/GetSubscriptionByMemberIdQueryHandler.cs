using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Application.Features.Subscriptions.Mappers;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Subscriptions.Queries.GetSubscriptionById;

public class GetSubscriptionByMemberIdQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetSubscriptionByMemberIdQuery, Result<SubscriptionResponse>>
{
    public async Task<Result<SubscriptionResponse>> Handle(GetSubscriptionByMemberIdQuery request, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.Subscriptions.Include(s => s.Member).ThenInclude(m => m.Person).OrderByDescending(s => s.Id).Include(s => s.Plan).FirstOrDefaultAsync(s => s.MemberId == request.MemberId, cancellationToken);
        if (subscription is null)
        {
            return ApplicationErrors.SubscriptionNotFound;
        }

        return subscription.ToDto();
    }
}