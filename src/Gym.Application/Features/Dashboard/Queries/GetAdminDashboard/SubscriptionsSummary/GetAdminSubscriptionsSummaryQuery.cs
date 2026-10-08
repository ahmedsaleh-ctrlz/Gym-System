using System;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Subscriptions_Summary;

public class GetAdminSubscriptionsSummaryQuery : ICachedQuery<Result<AdminSubscriptionsSummaryResponse>>
{
    public string CacheKey => "AdminDashboard:SubscriptionsSummary";

    public string[] CacheTag => ["AdminDashboard:SubscriptionsSummary"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
