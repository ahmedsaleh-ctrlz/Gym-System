using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard;

public class GetAdminDashboardQuery : ICachedQuery<Result<AdminDashboardResponse>>
{
    public string CacheKey => "AdminDashboard";

    public string[] CacheTag => ["AdminDashboard"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}