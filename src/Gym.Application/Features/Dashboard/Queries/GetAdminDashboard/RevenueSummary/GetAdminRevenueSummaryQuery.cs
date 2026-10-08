using System;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.RevenueSummary;

public enum RevenuePeriod
{
    Month,
    Year
}

public sealed record GetAdminRevenueSummaryQuery(
    RevenuePeriod Period,
    int? Year = null,
    int? Month = null)
    : ICachedQuery<Result<AdminRevenueSummaryResponse>>
{
    public string CacheKey =>
        $"AdminDashboard:RevenueSummary:{Period}:{Year}:{Month}";

    public string[] CacheTag =>
        ["AdminDashboard:RevenueSummary"];

    public TimeSpan CacheDuration =>
        TimeSpan.FromMinutes(10);
}
