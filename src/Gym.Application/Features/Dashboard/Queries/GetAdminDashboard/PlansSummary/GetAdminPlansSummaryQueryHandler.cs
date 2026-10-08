using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PlansSummary;

public sealed class GetAdminPlansSummaryQueryHandler(
    IAppDbContext dbContext)
    : IRequestHandler<
        GetAdminPlansSummaryQuery,
        Result<AdminPlansSummaryResponse>>
{
    public async Task<Result<AdminPlansSummaryResponse>> Handle(
        GetAdminPlansSummaryQuery request,
        CancellationToken ct)
    {
        var plans = await dbContext.Plans
            .Select(plan => new AdminPlanPerformance(
                plan.Id,
                plan.Title,
                dbContext.Subscriptions
                    .Count(subscription =>
                        subscription.PlanId == plan.Id),
                dbContext.Payments.Where(payment => payment.Status == PaymentStatus.Paid && payment.Subscription.PlanId == plan.Id).Sum(payment => (decimal?)payment.Amount) ?? 0))
            .ToListAsync(ct);

        var mostPopularPlan = plans
            .OrderByDescending(plan => plan.SubscriptionsCount)
            .FirstOrDefault();

        var highestRevenuePlan = plans
            .OrderByDescending(plan => plan.Revenue)
            .FirstOrDefault();

        var response = new AdminPlansSummaryResponse(
            MostPopularPlan:
                mostPopularPlan?.PlanName ?? string.Empty,
            MostPopularPlanSubscriptionsCount: mostPopularPlan?.SubscriptionsCount ?? 0,
            HighestRevenuePlan: highestRevenuePlan?.PlanName ?? string.Empty,
            HighestPlanRevenue: highestRevenuePlan?.Revenue ?? 0,
            Plans: plans);

        return response;
    }
}