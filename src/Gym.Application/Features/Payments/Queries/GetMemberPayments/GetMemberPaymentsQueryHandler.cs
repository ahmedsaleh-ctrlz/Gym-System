using FluentValidation;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Payments.Queries.GetMemberPayments
{
    public sealed class GetMemberPaymentsQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetMemberPaymentsQuery, Result<List<PaymentResponse>>>
    {
        public async Task<Result<List<PaymentResponse>>> Handle(GetMemberPaymentsQuery request, CancellationToken cancellationToken)
        {
            var payments = await dbContext.Payments.AsNoTracking().Include(p => p.Subscription)
                .Where(p => p.Subscription.MemberId == request.MemberId)
                .OrderByDescending(p => p.PaidAtUtc)
                .Select(p => new PaymentResponse
                {
                    PaymentId = p.Id,
                    SubscriptionId = p.SubscriptionId,
                    MemberId = p.Subscription.MemberId,
                    MemberName = p.Subscription.Member.Person.FirstName + " " + p.Subscription.Member.Person.LastName,
                    PlanName = p.Subscription.Plan!.Title,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod!.ToString(),
                    Status = p.Status.ToString(),
                    PaidAtUtc = p.PaidAtUtc
                })
                .ToListAsync(cancellationToken);

            return payments;
        }
    }
}