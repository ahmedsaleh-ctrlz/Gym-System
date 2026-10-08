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

        var month =
            request.Month ??
            (request.Period == AttendancePeriod.Month
                ? now.Month
                : 1);

        if (request.Period == AttendancePeriod.Month)
        {
            var startOfMonth = new DateTime(year, month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);
            var startOfPreviousMonth = startOfMonth.AddMonths(-1);

            var currentAttendance =
                await dbContext.Attendances
                    .CountAsync(
                        a => a.CheckInAtUtc >= startOfMonth &&
                             a.CheckInAtUtc < startOfNextMonth,
                        ct);

            var previousAttendance =
                await dbContext.Attendances
                    .CountAsync(
                        a => a.CheckInAtUtc >= startOfPreviousMonth &&
                             a.CheckInAtUtc < startOfMonth,
                        ct);

            var attendanceData =
                await dbContext.Attendances
                    .Where(a =>
                        a.CheckInAtUtc >= startOfMonth &&
                        a.CheckInAtUtc < startOfNextMonth)
                    .GroupBy(a => DateOnly.FromDateTime(a.CheckInAtUtc))
                    .Select(g => new AttendancePoint
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync(ct);

            var daysInMonth =
                DateTime.DaysInMonth(year, month);

            var data =
                Enumerable
                    .Range(1, daysInMonth)
                    .Select(day =>
                    {
                        var date = new DateOnly(year, month, day);

                        var existing =
                            attendanceData.FirstOrDefault(
                                x => x.Date == date);

                        return new AttendancePoint
                        {
                            Date = date,
                            Count = existing?.Count ?? 0
                        };
                    })
                    .ToList();

            var peakHourData =
                await dbContext.Attendances
                    .Where(a =>
                        a.CheckInAtUtc >= startOfMonth &&
                        a.CheckInAtUtc < startOfNextMonth)
                    .GroupBy(a => a.CheckInAtUtc.Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync(ct);

            var averageDailyAttendance =
                daysInMonth == 0
                    ? 0
                    : (decimal)currentAttendance / daysInMonth;

            return new AdminAttendanceSummaryResponse(
                CurrentPeriodAttendance: currentAttendance,
                PreviousPeriodAttendance: previousAttendance,
                GrowthPercentage: CalculateGrowth(
                    currentAttendance,
                    previousAttendance),
                AverageDailyAttendance: Math.Round(
                    averageDailyAttendance,
                    2),
                PeakHour: peakHourData?.Hour ?? 0,
                PeakHourAttendanceCount: peakHourData?.Count ?? 0,
                Data: data);
        }

        var startOfYear =
            new DateTime(year, 1, 1);

        var startOfNextYear =
            startOfYear.AddYears(1);

        var startOfPreviousYear =
            startOfYear.AddYears(-1);

        var currentAttendanceYear =
            await dbContext.Attendances
                .CountAsync(
                    a => a.CheckInAtUtc >= startOfYear &&
                         a.CheckInAtUtc < startOfNextYear,
                    ct);

        var previousAttendanceYear =
            await dbContext.Attendances
                .CountAsync(
                    a => a.CheckInAtUtc >= startOfPreviousYear &&
                         a.CheckInAtUtc < startOfYear,
                    ct);

        var monthlyAttendance =
            await dbContext.Attendances
                .Where(a =>
                    a.CheckInAtUtc >= startOfYear &&
                    a.CheckInAtUtc < startOfNextYear)
                .GroupBy(a => new
                {
                    a.CheckInAtUtc.Year,
                    a.CheckInAtUtc.Month
                })
                .Select(g => new AttendancePoint
                {
                    Date = new DateOnly(
                        g.Key.Year,
                        g.Key.Month,
                        1),
                    Count = g.Count()
                })
                .ToListAsync(ct);

        var dataYear =
            Enumerable
                .Range(1, 12)
                .Select(monthNumber =>
                {
                    var date =
                        new DateOnly(
                            year,
                            monthNumber,
                            1);

                    var existing =
                        monthlyAttendance.FirstOrDefault(
                            x => x.Date == date);

                    return new AttendancePoint
                    {
                        Date = date,
                        Count = existing?.Count ?? 0
                    };
                })
                .ToList();

        var peakHourYearData =
            await dbContext.Attendances
                .Where(a =>
                    a.CheckInAtUtc >= startOfYear &&
                    a.CheckInAtUtc < startOfNextYear)
                .GroupBy(a => a.CheckInAtUtc.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);

        var daysInYear =
            DateTime.IsLeapYear(year)
                ? 366
                : 365;

        var averageDailyAttendanceYear =
            (decimal)currentAttendanceYear / daysInYear;

        return new AdminAttendanceSummaryResponse(
            CurrentPeriodAttendance: currentAttendanceYear,
            PreviousPeriodAttendance: previousAttendanceYear,
            GrowthPercentage: CalculateGrowth(
                currentAttendanceYear,
                previousAttendanceYear),
            AverageDailyAttendance: Math.Round(
                averageDailyAttendanceYear,
                2),
            PeakHour: peakHourYearData?.Hour ?? 0,
            PeakHourAttendanceCount: peakHourYearData?.Count ?? 0,
            Data: dataYear);
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