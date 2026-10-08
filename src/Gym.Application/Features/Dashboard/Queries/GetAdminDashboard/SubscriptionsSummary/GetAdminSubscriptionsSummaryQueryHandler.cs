using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Subscriptions_Summary;

public class GetAdminSubscriptionsSummaryQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetAdminSubscriptionsSummaryQuery, Result<AdminSubscriptionsSummaryResponse>>
{
    public async Task<Result<AdminSubscriptionsSummaryResponse>> Handle(GetAdminSubscriptionsSummaryQuery request, CancellationToken ct)
    {
    var now = DateTime.UtcNow;
    var startOfMonth = new DateTime(now.Year, now.Month, 1);
    var startOfNextMonth = startOfMonth.AddMonths(1);

    var activeSubscriptions =
            await dbContext.Subscriptions
                .CountAsync(
                    s => s.Status == SubscriptionStatus.Active,
                    ct);
    var frozenSubscriptions =
            await dbContext.Subscriptions
                .CountAsync(
                    s => s.Status == SubscriptionStatus.Frozen,
                    ct);

    var scheduledSubscriptions =
            await dbContext.Subscriptions
                .CountAsync(
                    s => s.Status == SubscriptionStatus.Scheduled,
                    ct);

    var expiredSubscriptions =
            await dbContext.Subscriptions
                .CountAsync(
                    s => s.Status == SubscriptionStatus.Expired,
                    ct);
    var newSubscriptionsThisMonth =
            await dbContext.Payments
                .Where(p =>
                    p.Status == PaymentStatus.Paid &&
                    p.PaidAtUtc >= startOfMonth &&
                    p.PaidAtUtc < startOfNextMonth)
                .Select(p => p.SubscriptionId)
                .Distinct()
                .CountAsync(ct);

    var newSubscriptionsPreviousMonth =
            await dbContext.Payments
                .Where(p =>
                    p.Status == PaymentStatus.Paid &&
                    p.PaidAtUtc >= startOfMonth.AddMonths(-1) &&
                    p.PaidAtUtc < startOfMonth)
                .Select(p => p.SubscriptionId)
                .Distinct()
                .CountAsync(ct);

    var expiringSubscriptionsNext7Days =
            await dbContext.Subscriptions
                .CountAsync(
                    s => s.Status == SubscriptionStatus.Active &&
                    s.EndDate >= DateOnly.FromDateTime(now) &&
                    s.EndDate <= DateOnly.FromDateTime(now.AddDays(7)),
                    ct);

    var expiringSubscriptionsNext30Days =
            await dbContext.Subscriptions
                .CountAsync(
                    s => s.Status == SubscriptionStatus.Active &&
                    s.EndDate >= DateOnly.FromDateTime(now) &&
                    s.EndDate <= DateOnly.FromDateTime(now.AddDays(30)),
                    ct);
    var expiredSubscriptionsThisMonth =
    await dbContext.Subscriptions.CountAsync(
        s => s.Status == SubscriptionStatus.Expired &&
             s.EndDate >= DateOnly.FromDateTime(startOfMonth) &&
             s.EndDate < DateOnly.FromDateTime(startOfNextMonth),
        ct);

    return new AdminSubscriptionsSummaryResponse(
        ActiveSubscriptions: activeSubscriptions,
        FrozenSubscriptions: frozenSubscriptions,
        ScheduledSubscriptions: scheduledSubscriptions,
        ExpiredSubscriptions: expiredSubscriptions,
        NewSubscriptionsThisMonth: newSubscriptionsThisMonth,
        NewSubscriptionsPreviousMonth: newSubscriptionsPreviousMonth,
        ExpiringSubscriptionsNext7Days: expiringSubscriptionsNext7Days,
        ExpiringSubscriptionsNext30Days: expiringSubscriptionsNext30Days,
        ExpiredSubscriptionsThisMonth: expiredSubscriptionsThisMonth);
    }
}