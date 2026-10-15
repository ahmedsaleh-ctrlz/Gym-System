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

            var isCurrentMonth =
                year == now.Year && month == now.Month;

            // Current month: include only dates up to today.
            // Historical month: include the entire selected month.
            var currentEndExclusive = isCurrentMonth
                ? now.Date.AddDays(1)
                : startOfNextMonth;

            var currentDays =
                (currentEndExclusive.Date - startOfMonth.Date).Days;

            // Compare the same elapsed day range when viewing
            // the current month. For historical months, compare
            // the complete calendar months.
            var previousEndExclusive = isCurrentMonth
                ? startOfPreviousMonth.AddDays(
                    Math.Min(
                        currentDays,
                        DateTime.DaysInMonth(
                            startOfPreviousMonth.Year,
                            startOfPreviousMonth.Month)))
                : startOfMonth;

            var currentDailyRevenue = await GetDailyRevenueAsync(
                startOfMonth,
                currentEndExclusive,
                ct);

            var previousDailyRevenue = await GetDailyRevenueAsync(
                startOfPreviousMonth,
                previousEndExclusive,
                ct);

            var currentData = BuildDailyData(
                startOfMonth,
                currentEndExclusive,
                currentDailyRevenue);

            var previousData = BuildDailyData(
                startOfPreviousMonth,
                previousEndExclusive,
                previousDailyRevenue);

            var currentRevenue = currentData.Sum(x => x.Amount);
            var previousRevenue = previousData.Sum(x => x.Amount);

            return new AdminRevenueSummaryResponse(
                CurrentPeriodRevenue: currentRevenue,
                PreviousPeriodRevenue: previousRevenue,
                GrowthPercentage: CalculateGrowth(
                    currentRevenue,
                    previousRevenue),
                Data: currentData,
                PreviousPeriodData: previousData);
        }

        var startOfYear = new DateTime(year, 1, 1);
        var startOfNextYear = startOfYear.AddYears(1);
        var startOfPreviousYear = startOfYear.AddYears(-1);

        var isCurrentYear = year == now.Year;

        // For the current year, compare year-to-date revenue
        // with the same elapsed period in the previous year.
        // For historical years, compare both complete years.
        var currentComparisonEndExclusive = isCurrentYear
            ? now.Date.AddDays(1)
            : startOfNextYear;

        var previousComparisonEndExclusive = isCurrentYear
            ? GetSameDateInPreviousYear(now).AddDays(1)
            : startOfYear;

        var currentYearRevenue = await GetRevenueTotalAsync(
            startOfYear,
            currentComparisonEndExclusive,
            ct);

        var previousYearRevenue = await GetRevenueTotalAsync(
            startOfPreviousYear,
            previousComparisonEndExclusive,
            ct);

        // Both chart series always contain all 12 months.
        var currentMonthlyRevenue = await GetMonthlyRevenueAsync(
            startOfYear,
            startOfNextYear,
            ct);

        var previousMonthlyRevenue = await GetMonthlyRevenueAsync(
            startOfPreviousYear,
            startOfYear,
            ct);

        var currentYearData = BuildMonthlyData(
            year,
            currentMonthlyRevenue);

        var previousYearData = BuildMonthlyData(
            year - 1,
            previousMonthlyRevenue);

        return new AdminRevenueSummaryResponse(
            CurrentPeriodRevenue: currentYearRevenue,
            PreviousPeriodRevenue: previousYearRevenue,
            GrowthPercentage: CalculateGrowth(
                currentYearRevenue,
                previousYearRevenue),
            Data: currentYearData,
            PreviousPeriodData: previousYearData);
    }

    private async Task<Dictionary<DateOnly, decimal>> GetDailyRevenueAsync(
        DateTime start,
        DateTime endExclusive,
        CancellationToken ct)
    {
        var rows = await dbContext.Payments
            .Where(p =>
                p.Status == PaymentStatus.Paid &&
                p.PaidAtUtc.HasValue &&
                p.PaidAtUtc.Value >= start &&
                p.PaidAtUtc.Value < endExclusive)
            .GroupBy(p => p.PaidAtUtc!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Amount = g.Sum(p => p.Amount)
            })
            .ToListAsync(ct);

        return rows.ToDictionary(
            x => DateOnly.FromDateTime(x.Date),
            x => x.Amount);
    }

    private async Task<Dictionary<DateOnly, decimal>> GetMonthlyRevenueAsync(
        DateTime start,
        DateTime endExclusive,
        CancellationToken ct)
    {
        var rows = await dbContext.Payments
            .Where(p =>
                p.Status == PaymentStatus.Paid &&
                p.PaidAtUtc.HasValue &&
                p.PaidAtUtc.Value >= start &&
                p.PaidAtUtc.Value < endExclusive)
            .GroupBy(p => new
            {
                p.PaidAtUtc!.Value.Year,
                p.PaidAtUtc.Value.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Amount = g.Sum(p => p.Amount)
            })
            .ToListAsync(ct);

        return rows.ToDictionary(
            x => new DateOnly(x.Year, x.Month, 1),
            x => x.Amount);
    }

    private async Task<decimal> GetRevenueTotalAsync(
        DateTime start,
        DateTime endExclusive,
        CancellationToken ct)
    {
        return await dbContext.Payments
            .Where(p =>
                p.Status == PaymentStatus.Paid &&
                p.PaidAtUtc.HasValue &&
                p.PaidAtUtc.Value >= start &&
                p.PaidAtUtc.Value < endExclusive)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
    }

    private static List<RevenuePoint> BuildDailyData(
        DateTime start,
        DateTime endExclusive,
        IReadOnlyDictionary<DateOnly, decimal> revenueByDate)
    {
        var daysCount =
            (endExclusive.Date - start.Date).Days;

        return Enumerable
            .Range(0, daysCount)
            .Select(offset =>
            {
                var date = DateOnly.FromDateTime(
                    start.Date.AddDays(offset));

                return new RevenuePoint
                {
                    Date = date,
                    Amount = revenueByDate.GetValueOrDefault(date)
                };
            })
            .ToList();
    }

    private static List<RevenuePoint> BuildMonthlyData(
        int year,
        IReadOnlyDictionary<DateOnly, decimal> revenueByMonth)
    {
        return Enumerable
            .Range(1, 12)
            .Select(month =>
            {
                var date = new DateOnly(year, month, 1);

                return new RevenuePoint
                {
                    Date = date,
                    Amount = revenueByMonth.GetValueOrDefault(date)
                };
            })
            .ToList();
    }

    private static DateTime GetSameDateInPreviousYear(DateTime date)
    {
        var previousYear = date.Year - 1;
        var day = Math.Min(
            date.Day,
            DateTime.DaysInMonth(previousYear, date.Month));

        return new DateTime(
            previousYear,
            date.Month,
            day);
    }

    private static decimal CalculateGrowth(
        decimal current,
        decimal previous)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : 100;
        }

        return Math.Round(
            (current - previous) / previous * 100,
            2);
    }
}