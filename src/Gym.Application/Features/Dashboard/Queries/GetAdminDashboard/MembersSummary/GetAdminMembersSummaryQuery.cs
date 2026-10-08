using System;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.MembersSummary;

public sealed record GetAdminMembersSummaryQuery
    : ICachedQuery<Result<AdminMembersSummaryResponse>>
{
    public string CacheKey => "AdminDashboard:MembersSummary";

    public string[] CacheTag => ["AdminDashboard:MembersSummary"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}