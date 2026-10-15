using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.AttendanceSummary;

public sealed class GetAdminAttendanceSummaryQueryHandler(
    IAppDbContext dbContext)
    : IRequestHandler<
        GetAdminAttendanceSummaryQuery,
        Result<AdminAttendanceSummaryResponse>>
{
    public async Task<Result<AdminAttendanceSummaryResponse>> Handle(
        GetAdminAttendanceSummaryQuery request,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var year = request.Year ?? now.Year;

        if (request.Period == AttendancePeriod.Month)
        {
            var month = request.Month ?? now.Month;

            var startOfMonth = new DateTime(year, month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);
            var startOfPreviousMonth = startOfMonth.AddMonths(-1);

            var isCurrentMonth =
                year == now.Year && month == now.Month;

            var currentEndExclusive = isCurrentMonth
                ? now.Date.AddDays(1)
                : startOfNextMonth;

            var currentDays =
                (currentEndExclusive.Date - startOfMonth.Date).Days;

            var previousEndExclusive = isCurrentMonth
                ? startOfPreviousMonth.AddDays(
                    Math.Min(
                        currentDays,
                        DateTime.DaysInMonth(
                            startOfPreviousMonth.Year,
                            startOfPreviousMonth.Month)))
                : startOfMonth;

            var currentDailyAttendance =
                await GetDailyAttendanceAsync(
                    startOfMonth,
                    currentEndExclusive,
                    ct);

            var previousDailyAttendance =
                await GetDailyAttendanceAsync(
                    startOfPreviousMonth,
                    previousEndExclusive,
                    ct);

            var currentMonthData = BuildDailyData(
                startOfMonth,
                currentEndExclusive,
                currentDailyAttendance);

            var previousMonthData = BuildDailyData(
                startOfPreviousMonth,
                previousEndExclusive,
                previousDailyAttendance);

            var currentAttendance =
                currentMonthData.Sum(x => x.Count);

            var previousAttendance =
                previousMonthData.Sum(x => x.Count);

            var monthlyPeakHourData =
                await dbContext.Attendances
                    .Where(a =>
                        a.CheckInAtUtc >= startOfMonth &&
                        a.CheckInAtUtc < currentEndExclusive)
                    .GroupBy(a => a.CheckInAtUtc.Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync(ct);

            var averageDailyAttendance = currentDays > 0
                ? Math.Round(
                    (decimal)currentAttendance / currentDays,
                    2)
                : 0m;

            return new AdminAttendanceSummaryResponse(
                CurrentPeriodAttendance: currentAttendance,
                PreviousPeriodAttendance: previousAttendance,
                GrowthPercentage: CalculateGrowth(
                    currentAttendance,
                    previousAttendance),
                AverageDailyAttendance: averageDailyAttendance,
                PeakHour: monthlyPeakHourData?.Hour ?? 0,
                PeakHourAttendanceCount:
                    monthlyPeakHourData?.Count ?? 0,
                Data: currentMonthData,
                PreviousPeriodData: previousMonthData);
        }

        var startOfYear = new DateTime(year, 1, 1);
        var startOfNextYear = startOfYear.AddYears(1);
        var startOfPreviousYear = startOfYear.AddYears(-1);

        var isCurrentYear = year == now.Year;

        var currentYearEndExclusive = isCurrentYear
            ? now.Date.AddDays(1)
            : startOfNextYear;

        var previousYearEndExclusive = isCurrentYear
            ? GetSameDateInPreviousYear(now).Date.AddDays(1)
            : startOfYear;

        var currentYearAttendance =
            await dbContext.Attendances
                .CountAsync(
                    a => a.CheckInAtUtc >= startOfYear &&
                         a.CheckInAtUtc < currentYearEndExclusive,
                    ct);

        var previousYearAttendance =
            await dbContext.Attendances
                .CountAsync(
                    a => a.CheckInAtUtc >= startOfPreviousYear &&
                         a.CheckInAtUtc < previousYearEndExclusive,
                    ct);

        // Return 12 monthly points for both years.
        var currentMonthlyAttendance =
            await GetMonthlyAttendanceAsync(
                startOfYear,
                startOfNextYear,
                ct);

        var previousMonthlyAttendance =
            await GetMonthlyAttendanceAsync(
                startOfPreviousYear,
                startOfYear,
                ct);

        var currentYearData = BuildMonthlyData(
            year,
            currentMonthlyAttendance);

        var previousYearData = BuildMonthlyData(
            year - 1,
            previousMonthlyAttendance);

        var yearlyPeakHourData =
            await dbContext.Attendances
                .Where(a =>
                    a.CheckInAtUtc >= startOfYear &&
                    a.CheckInAtUtc < currentYearEndExclusive)
                .GroupBy(a => a.CheckInAtUtc.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);

        var elapsedDays =
            (currentYearEndExclusive.Date - startOfYear.Date).Days;

        var averageDailyAttendanceYear = elapsedDays > 0
            ? Math.Round(
                (decimal)currentYearAttendance / elapsedDays,
                2)
            : 0m;

        return new AdminAttendanceSummaryResponse(
            CurrentPeriodAttendance: currentYearAttendance,
            PreviousPeriodAttendance: previousYearAttendance,
            GrowthPercentage: CalculateGrowth(
                currentYearAttendance,
                previousYearAttendance),
            AverageDailyAttendance: averageDailyAttendanceYear,
            PeakHour: yearlyPeakHourData?.Hour ?? 0,
            PeakHourAttendanceCount:
                yearlyPeakHourData?.Count ?? 0,
            Data: currentYearData,
            PreviousPeriodData: previousYearData);
    }

    private async Task<Dictionary<DateOnly, int>> GetDailyAttendanceAsync(
        DateTime start,
        DateTime endExclusive,
        CancellationToken ct)
    {
        var rows = await dbContext.Attendances
            .Where(a =>
                a.CheckInAtUtc >= start &&
                a.CheckInAtUtc < endExclusive)
            .GroupBy(a => DateOnly.FromDateTime(a.CheckInAtUtc))
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        return rows.ToDictionary(
            x => x.Date,
            x => x.Count);
    }

    private async Task<Dictionary<DateOnly, int>> GetMonthlyAttendanceAsync(
        DateTime start,
        DateTime endExclusive,
        CancellationToken ct)
    {
        var rows = await dbContext.Attendances
            .Where(a =>
                a.CheckInAtUtc >= start &&
                a.CheckInAtUtc < endExclusive)
            .GroupBy(a => new
            {
                a.CheckInAtUtc.Year,
                a.CheckInAtUtc.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync(ct);

        return rows.ToDictionary(
            x => new DateOnly(x.Year, x.Month, 1),
            x => x.Count);
    }

    private static List<AttendancePoint> BuildDailyData(
        DateTime start,
        DateTime endExclusive,
        IReadOnlyDictionary<DateOnly, int> attendanceByDate)
    {
        var daysCount =
            (endExclusive.Date - start.Date).Days;

        return Enumerable
            .Range(0, daysCount)
            .Select(offset =>
            {
                var date = DateOnly.FromDateTime(
                    start.Date.AddDays(offset));

                return new AttendancePoint
                {
                    Date = date,
                    Count = attendanceByDate.GetValueOrDefault(date)
                };
            })
            .ToList();
    }

    private static List<AttendancePoint> BuildMonthlyData(
        int year,
        IReadOnlyDictionary<DateOnly, int> attendanceByMonth)
    {
        return Enumerable
            .Range(1, 12)
            .Select(month =>
            {
                var date = new DateOnly(year, month, 1);

                return new AttendancePoint
                {
                    Date = date,
                    Count = attendanceByMonth.GetValueOrDefault(date)
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
        int current,
        int previous)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : 100;
        }

        return Math.Round(
            (decimal)(current - previous) / previous * 100,
            2);
    }
}