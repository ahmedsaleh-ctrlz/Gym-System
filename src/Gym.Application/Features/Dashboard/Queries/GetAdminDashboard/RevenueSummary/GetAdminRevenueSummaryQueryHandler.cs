using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.RevenueSummary;

public sealed class GetAdminRevenueSummaryQueryHandler(
    IAppDbContext dbContext)
    : IRequestHandler<
        GetAdminRevenueSummaryQuery,
        Result<AdminRevenueSummaryResponse>>
{
    public async Task<Result<AdminRevenueSummaryResponse>> Handle(
        GetAdminRevenueSummaryQuery request,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var year = request.Year ?? now.Year;

        if (request.Period == RevenuePeriod.Month)
        {
            var month = request.Month ?? now.Month;

            var startOfMonth = new DateTime(year, month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);
            var startOfPreviousMonth = startOfMonth.AddMonths(-1);

            var currentRevenue =
                await dbContext.Payments
                    .Where(p =>
                        p.Status == PaymentStatus.Paid &&
                        p.PaidAtUtc.HasValue &&
                        p.PaidAtUtc.Value >= startOfMonth &&
                        p.PaidAtUtc.Value < startOfNextMonth)
                    .SumAsync(
                        p => (decimal?)p.Amount,
                        ct) ?? 0m;

            var previousRevenue =
                await dbContext.Payments
                    .Where(p =>
                        p.Status == PaymentStatus.Paid &&
                        p.PaidAtUtc.HasValue &&
                        p.PaidAtUtc.Value >= startOfPreviousMonth &&
                        p.PaidAtUtc.Value < startOfMonth)
                    .SumAsync(
                        p => (decimal?)p.Amount,
                        ct) ?? 0m;

            var dailyRevenue =
                await dbContext.Payments
                    .Where(p =>
                        p.Status == PaymentStatus.Paid &&
                        p.PaidAtUtc.HasValue &&
                        p.PaidAtUtc.Value >= startOfMonth &&
                        p.PaidAtUtc.Value < startOfNextMonth)
                    .GroupBy(p =>
                        DateOnly.FromDateTime(p.PaidAtUtc!.Value))
                    .Select(g => new RevenuePoint
                    {
                        Date = g.Key,
                        Amount = g.Sum(p => p.Amount)
                    })
                    .ToListAsync(ct);

            var dailyRevenueByDate =
                dailyRevenue.ToDictionary(
                    x => x.Date,
                    x => x.Amount);

            var daysInMonth =
                DateTime.DaysInMonth(year, month);

            var dailyData =
                Enumerable
                    .Range(1, daysInMonth)
                    .Select(day =>
                    {
                        var date = new DateOnly(
                            year,
                            month,
                            day);

                        return new RevenuePoint
                        {
                            Date = date,
                            Amount = dailyRevenueByDate
                                .GetValueOrDefault(date)
                        };
                    })
                    .ToList();

            return new AdminRevenueSummaryResponse(
                CurrentPeriodRevenue: currentRevenue,
                PreviousPeriodRevenue: previousRevenue,
                GrowthPercentage: CalculateGrowth(
                    currentRevenue,
                    previousRevenue),
                Data: dailyData);
        }

        var startOfYear =
            new DateTime(year, 1, 1);

        var startOfNextYear =
            startOfYear.AddYears(1);

        var startOfPreviousYear =
            startOfYear.AddYears(-1);

        var currentYearRevenue =
            await dbContext.Payments
                .Where(p =>
                    p.Status == PaymentStatus.Paid &&
                    p.PaidAtUtc.HasValue &&
                    p.PaidAtUtc.Value >= startOfYear &&
                    p.PaidAtUtc.Value < startOfNextYear)
                .SumAsync(
                    p => (decimal?)p.Amount,
                    ct) ?? 0m;

        var previousYearRevenue =
            await dbContext.Payments
                .Where(p =>
                    p.Status == PaymentStatus.Paid &&
                    p.PaidAtUtc.HasValue &&
                    p.PaidAtUtc.Value >= startOfPreviousYear &&
                    p.PaidAtUtc.Value < startOfYear)
                .SumAsync(
                    p => (decimal?)p.Amount,
                    ct) ?? 0m;

        var monthlyRevenue =
            await dbContext.Payments
                .Where(p =>
                    p.Status == PaymentStatus.Paid &&
                    p.PaidAtUtc.HasValue &&
                    p.PaidAtUtc.Value >= startOfYear &&
                    p.PaidAtUtc.Value < startOfNextYear)
                .GroupBy(p => new
                {
                    p.PaidAtUtc!.Value.Year,
                    p.PaidAtUtc.Value.Month
                })
                .Select(g => new RevenuePoint
                {
                    Date = new DateOnly(
                        g.Key.Year,
                        g.Key.Month,
                        1),
                    Amount = g.Sum(p => p.Amount)
                })
                .ToListAsync(ct);

        var monthlyRevenueByDate =
            monthlyRevenue.ToDictionary(
                x => x.Date,
                x => x.Amount);

        var monthlyData =
            Enumerable
                .Range(1, 12)
                .Select(monthNumber =>
                {
                    var date = new DateOnly(
                        year,
                        monthNumber,
                        1);

                    return new RevenuePoint
                    {
                        Date = date,
                        Amount = monthlyRevenueByDate
                            .GetValueOrDefault(date)
                    };
                })
                .ToList();

        return new AdminRevenueSummaryResponse(
            CurrentPeriodRevenue: currentYearRevenue,
            PreviousPeriodRevenue: previousYearRevenue,
            GrowthPercentage: CalculateGrowth(
                currentYearRevenue,
                previousYearRevenue),
            Data: monthlyData);
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
            (current - previous) / previous * 100,
            2);
    }
}