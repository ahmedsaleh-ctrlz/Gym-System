using System;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PlansSummary;

public sealed class GetAdminPlansSummaryQuery
    : ICachedQuery<Result<AdminPlansSummaryResponse>>
{
    public string CacheKey => "AdminDashboard:PlansSummary";

    public string[] CacheTag => ["AdminDashboard:PlansSummary"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
