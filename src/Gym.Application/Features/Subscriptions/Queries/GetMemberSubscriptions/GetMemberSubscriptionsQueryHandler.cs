using FluentValidation;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Dtos;
using Gym.Application.Features.Payments.Queries.GetMemberPayments;
using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Gym.Application.Features.Subscriptions.Queries.GetMemberSubscriptions
{
    public sealed class GetMemberSubscriptionsQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetMemberSubscriptionsQuery, Result<List<SubscriptionResponse>>>
    {
        public async Task<Result<List<SubscriptionResponse>>> Handle(GetMemberSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            var subscriptions = await dbContext.Subscriptions.AsNoTracking().OrderByDescending(x => x.EndDate).Where(s => s.MemberId == request.MemberId)
                .Select(s => new SubscriptionResponse
                {
                    SubscriptionId = s.Id,
                    MemberId = request.MemberId,
                    MemberName = s.Member.Person.FirstName + " " + s.Member.Person.LastName,
                    PlanName = s.Plan!.Title,
                    PriceSnapshot = s.PriceSnapshot,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    FreezeCountUsed = s.FreezeCountUsed,
                    TotalFreezeDaysUsed = s.TotalFreezeDaysUsed,
                    Status = s.Status.ToString()


                }).ToListAsync(cancellationToken);

            return subscriptions;
        }


        
    }
}
