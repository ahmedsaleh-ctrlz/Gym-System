using System;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PaymentsSummary;

public sealed class GetAdminPaymentsSummaryQuery
    : ICachedQuery<Result<AdminPaymentsSummaryResponse>>
{
    public string CacheKey => "AdminDashboard:PaymentsSummary";

    public string[] CacheTag => ["AdminDashboard:PaymentsSummary"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
