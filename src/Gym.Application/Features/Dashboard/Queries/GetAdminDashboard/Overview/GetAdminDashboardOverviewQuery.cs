using System;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Overview;

public sealed class GetAdminDashboardOverviewQuery
    : ICachedQuery<Result<AdminDashboardOverviewResponse>>
{
    public string CacheKey => "AdminDashboard:Overview";

    public string[] CacheTag => ["AdminDashboard:Overview"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
