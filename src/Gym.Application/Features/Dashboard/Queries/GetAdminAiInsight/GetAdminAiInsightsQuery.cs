using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminAiInsight;

public sealed record GetAdminAiInsightsQuery
    : ICachedQuery<Result<AdminAiInsightsResponse>>
{
    public string CacheKey => "AdminDashboard:AiInsights";

    public string[] CacheTag => ["AdminDashboard:AiInsights"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}