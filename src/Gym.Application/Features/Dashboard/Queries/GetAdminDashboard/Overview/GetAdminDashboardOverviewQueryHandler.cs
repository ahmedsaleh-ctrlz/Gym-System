using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Overview;

public sealed class GetAdminDashboardOverviewQueryHandler(
    IAppDbContext dbContext)
    : IRequestHandler<
        GetAdminDashboardOverviewQuery,
        Result<AdminDashboardOverviewResponse>>
{
    public async Task<Result<AdminDashboardOverviewResponse>> Handle(
        GetAdminDashboardOverviewQuery request,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var startOfMonth =
            new DateTime(now.Year, now.Month, 1);

        var startOfNextMonth =
            startOfMonth.AddMonths(1);

        var startOfPreviousMonth =
            startOfMonth.AddMonths(-1);

        var expiringUntil =
            today.AddDays(7);

        var totalMembers =
            await dbContext.Members
                .CountAsync(ct);

        var activeSubscriptions =
            await dbContext.Subscriptions
                .CountAsync(
                    s => s.Status == SubscriptionStatus.Active,
                    ct);

        var expiringSubscriptionsNext7Days =
            await dbContext.Subscriptions
                .CountAsync(
                    s =>
                        s.Status == SubscriptionStatus.Active &&
                        s.EndDate >= today &&
                        s.EndDate <= expiringUntil,
                    ct);

        var thisMonthRevenue =
            await dbContext.Payments
                .Where(
                    p =>
                        p.Status == PaymentStatus.Paid &&
                        p.PaidAtUtc >= startOfMonth &&
                        p.PaidAtUtc < startOfNextMonth)
                .SumAsync(
                    p => (decimal?)p.Amount,
                    ct) ?? 0;

        var previousMonthRevenue =
            await dbContext.Payments
                .Where(
                    p =>
                        p.Status == PaymentStatus.Paid &&
                        p.PaidAtUtc >= startOfPreviousMonth &&
                        p.PaidAtUtc < startOfMonth)
                .SumAsync(
                    p => (decimal?)p.Amount,
                    ct) ?? 0;

        var revenueGrowthPercentage =
            CalculateGrowth(
                thisMonthRevenue,
                previousMonthRevenue);

        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);

        var todayAttendanceCount =
            await dbContext.Attendances
                .CountAsync(
                    a =>
                        a.CheckInAtUtc >= todayStart &&
                        a.CheckInAtUtc < tomorrowStart,
                    ct);

        var pendingPaymentsCount =
            await dbContext.Payments
                .CountAsync(
                    p => p.Status == PaymentStatus.Pending,
                    ct);

        var underReviewPaymentsCount =
            await dbContext.Payments
                .CountAsync(
                    p => p.Status == PaymentStatus.UnderReview,
                    ct);

        var mostPopularPlanData =
            await dbContext.Subscriptions
                .GroupBy(
                    s => new
                    {
                        s.PlanId,
                        PlanName = s.Plan!.Title
                    })
                .Select(
                    g => new
                    {
                        g.Key.PlanId,
                        g.Key.PlanName,
                        Count = g.Count()
                    })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);

        var response = new AdminDashboardOverviewResponse(
            TotalMembers: totalMembers,
            ActiveSubscriptions: activeSubscriptions,
            ExpiringSubscriptionsNext7Days: expiringSubscriptionsNext7Days,
            ThisMonthRevenue: thisMonthRevenue,
            PreviousMonthRevenue: previousMonthRevenue,
            RevenueGrowthPercentage: revenueGrowthPercentage,
            TodayAttendanceCount: todayAttendanceCount,
            PendingPaymentsCount: pendingPaymentsCount,
            UnderReviewPaymentsCount: underReviewPaymentsCount,
            MostPopularPlan: mostPopularPlanData?.PlanName ?? string.Empty,
            MostPopularPlanSubscriptionsCount: mostPopularPlanData?.Count ?? 0);

        return response;
    }

    private static decimal CalculateGrowth(
        decimal current,
        decimal previous)
    {
        if (previous == 0)
        {
            return current == 0
                ? 0
                : 100;
        }

        return Math.Round(
            ((current - previous) / previous) * 100,
            2);
    }
}