using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard;

public class GetAdminDashboardQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardResponse>>
{
    public async Task<Result<AdminDashboardResponse>> Handle(GetAdminDashboardQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayDateTime = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1);

        var totalMembers = await dbContext.Members.CountAsync(ct);

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

        var todayAttendanceCount =
            await dbContext.Attendances
                .CountAsync(
                    a => a.CheckInAtUtc.Date == todayDateTime,
                    ct);

        var thisWeekAttendanceCount =
            await dbContext.Attendances
                .CountAsync(
                    a => a.CheckInAtUtc >= DateTime.UtcNow.AddDays(-7),
                    ct);

        var peakHourData =
            await dbContext.Attendances
                .GroupBy(a => a.CheckInAtUtc.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);

        var todayRevenue =
           await dbContext.Payments
               .Where(p =>
                   p.Status == PaymentStatus.Paid
                   && p.PaidAtUtc.HasValue
                   && p.PaidAtUtc.Value.Date == todayDateTime)
               .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var thisMonthRevenue =
            await dbContext.Payments
                .Where(p =>
                    p.Status == PaymentStatus.Paid
                    && p.PaidAtUtc >= startOfMonth)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var totalRevenue =
            await dbContext.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var mostPopularPlanData =
           await dbContext.Subscriptions
               .GroupBy(s => s.Plan!.Title)
               .Select(g => new
               {
                   PlanName = g.Key,
                   Count = g.Count()
               })
               .OrderByDescending(x => x.Count)
               .FirstOrDefaultAsync(ct);

        var pendingPaymentsCount =
            await dbContext.Payments
                .CountAsync(
                    p => p.Status == PaymentStatus.Pending,
                    ct);

        var paidPaymentsCount =
            await dbContext.Payments
                .CountAsync(
                    p => p.Status == PaymentStatus.Paid,
                    ct);

        return new AdminDashboardResponse
        {
            TotalMembers = totalMembers,

            ActiveSubscriptions = activeSubscriptions,

            FrozenSubscriptions = frozenSubscriptions,

            ScheduledSubscriptions = scheduledSubscriptions,

            ExpiredSubscriptions = expiredSubscriptions,

            TodayAttendanceCount = todayAttendanceCount,

            ThisWeekAttendanceCount = thisWeekAttendanceCount,

            PeakHour = peakHourData?.Hour ?? 0,

            PeakHourAttendanceCount =
               peakHourData?.Count ?? 0,

            TodayRevenue = todayRevenue,

            ThisMonthRevenue = thisMonthRevenue,

            TotalRevenue = totalRevenue,

            MostPopularPlan =
               mostPopularPlanData?.PlanName ?? string.Empty,

            MostPopularPlanSubscriptionsCount =
               mostPopularPlanData?.Count ?? 0,

            PendingPaymentsCount = pendingPaymentsCount,

            PaidPaymentsCount = paidPaymentsCount,
        };
    }
}