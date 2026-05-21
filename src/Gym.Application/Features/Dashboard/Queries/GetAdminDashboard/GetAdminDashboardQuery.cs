using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard;

public class GetAdminDashboardQuery : ICachedQuery<Result<AdminDashboardResponse>>
{

    public string cacheKey => "AdminDashboard";

    public string[] cacheTag => ["AdminDashboard"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
