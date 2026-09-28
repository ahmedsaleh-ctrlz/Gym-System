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

        var revenueData =
            await dbContext.Payments.Where(p => p.PaidAtUtc.HasValue && p.Status == PaymentStatus.Paid && DateOnly.FromDateTime(p.PaidAtUtc.Value) >= today.AddDays(-6) ).
            GroupBy(p => DateOnly.FromDateTime(p.PaidAtUtc.Value)).
            Select(g => new RevenuePoint
            {
                Date = g.Key,
                Amount = g.Sum(p => p.Amount)
            }).ToListAsync(ct);

        var revenueResult = Enumerable.
            Range(0, 7).
            Select(i =>
            {
                var date = today.AddDays(-6 + i);
                var existing = revenueData
                    .FirstOrDefault(x => x.Date == date);
                return new RevenuePoint
                {
                    Date = date,
                    Amount = existing?.Amount ?? 0
                };
            })
            .ToList();

        var attendanceData = await dbContext.Attendances.Where(a => DateOnly.FromDateTime(a.CheckInAtUtc) >= today.AddDays(-6)).
            GroupBy(a => DateOnly.FromDateTime(a.CheckInAtUtc)).
            Select(g => new AttendancePoint
            {
                Date = g.Key,
                Count = g.Count()
            }).ToListAsync(ct);

        var attendanceResult = Enumerable
            .Range(0, 7)
            .Select(i =>
            {
                var date = today.AddDays(-6 + i);

                var existing = attendanceData
                    .FirstOrDefault(x => x.Date == date);

                return new AttendancePoint
                {
                    Date = date,
                    Count = existing?.Count ?? 0
                };
            })
            .ToList();

        var recentCheckIns = await dbContext.Attendances
            .OrderByDescending(a => a.CheckInAtUtc)
            .Take(5)
            .Select(a => new RecentCheckIn
            {
                MemberId = a.MemberId,
                MemberName = a.Member!.Person.FirstName + " " + a.Member!.Person.LastName,
                CheckInAtUtc = a.CheckInAtUtc,
                ImageUrl = a.Member!.Person.Image.ImageUrl,
            })
            .ToListAsync(ct);

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

            Revenue = revenueResult,

            Attendance = attendanceResult,

            RecentCheckIns = recentCheckIns,
        };
    }
}