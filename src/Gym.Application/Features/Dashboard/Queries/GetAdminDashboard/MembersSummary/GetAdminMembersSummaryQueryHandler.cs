using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.MembersSummary;

public class GetAdminMembersSummaryQueryHandler(
    IAppDbContext dbContext)
    : IRequestHandler<
        GetAdminMembersSummaryQuery,
        Result<AdminMembersSummaryResponse>>
{
    public async Task<Result<AdminMembersSummaryResponse>> Handle(
        GetAdminMembersSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var startOfCurrentMonth =
            new DateTime(now.Year, now.Month, 1);

        var startOfPreviousMonth =
            startOfCurrentMonth.AddMonths(-1);

        var startOfNextMonth =
            startOfCurrentMonth.AddMonths(1);

        var totalMembers =
            await dbContext.Members
                .CountAsync(cancellationToken);

        var newMembersThisMonth =
            await dbContext.Members
                .CountAsync(
                    m => m.JoinDate >= startOfCurrentMonth &&
                         m.JoinDate < startOfNextMonth,
                    cancellationToken);

        var newMembersPreviousMonth =
            await dbContext.Members
                .CountAsync(
                    m => m.JoinDate >= startOfPreviousMonth &&
                         m.JoinDate < startOfCurrentMonth,
                    cancellationToken);

        var response = new AdminMembersSummaryResponse(
            TotalMembers: totalMembers,
            NewMembersThisMonth: newMembersThisMonth,
            NewMembersPreviousMonth: newMembersPreviousMonth);

        return response;
    }
}