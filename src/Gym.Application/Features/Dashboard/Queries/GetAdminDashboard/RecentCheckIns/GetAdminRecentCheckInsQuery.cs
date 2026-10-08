using System;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.RecentCheckIns;
public sealed class GetAdminRecentCheckInsQuery
    : ICachedQuery<Result<AdminRecentCheckInsResponse>>
{
    public string CacheKey => "AdminDashboard:RecentCheckIns";

    public string[] CacheTag => ["AdminDashboard:RecentCheckIns"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}

public sealed class GetAdminRecentCheckInsQueryHandler(
    IAppDbContext dbContext)
    : IRequestHandler<
        GetAdminRecentCheckInsQuery,
        Result<AdminRecentCheckInsResponse>>
{
    public async Task<Result<AdminRecentCheckInsResponse>> Handle(
        GetAdminRecentCheckInsQuery request,
        CancellationToken ct)
    {
        var checkIns = await dbContext.Attendances
            .OrderByDescending(a => a.CheckInAtUtc)
            .Take(5)
            .Select(a => new RecentCheckIn
            {
                MemberId = a.MemberId,
                MemberName =
                    a.Member!.Person.FirstName +
                    " " +
                    a.Member.Person.LastName,
                ImageUrl = a.Member.Person.Image.ImageUrl,
                CheckInAtUtc = a.CheckInAtUtc
            })
            .ToListAsync(ct);

        return new AdminRecentCheckInsResponse(
            CheckIns: checkIns);
    }
}