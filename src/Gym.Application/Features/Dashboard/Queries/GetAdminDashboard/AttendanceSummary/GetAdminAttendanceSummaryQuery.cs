using System;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.AttendanceSummary;

public enum AttendancePeriod
{
    Month,
    Year
}

public sealed record GetAdminAttendanceSummaryQuery(
    AttendancePeriod Period,
    int? Year = null,
    int? Month = null)
    : ICachedQuery<Result<AdminAttendanceSummaryResponse>>
{
    public string CacheKey =>
        $"AdminDashboard:AttendanceSummary:{Period}:{Year}:{Month}";

    public string[] CacheTag =>
        ["AdminDashboard:AttendanceSummary"];

    public TimeSpan CacheDuration =>
        TimeSpan.FromMinutes(10);
}
