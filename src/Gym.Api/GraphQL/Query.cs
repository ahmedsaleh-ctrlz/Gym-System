using Gym.Application.Features.Dashboard.Dtos;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.AttendanceSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.MembersSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Overview;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PaymentsSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PlansSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.RecentCheckIns;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.RevenueSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Subscriptions_Summary;
using Gym.Domain.Identity;

using MediatR;

namespace Gym.Api.GraphQL;

public sealed class Query
{
    public string GetHello()
    {
        return "GraphQL is working";
    }

    [GraphQLDescription("Get admin dashboard statistics.")]
    [HotChocolate.Authorization.Authorize(Roles = [nameof(Role.Admin)])]
    public async Task<AdminDashboardOverviewResponse>
        GetAdminDashboardStatsAsync(
            ISender sender,
            CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminDashboardOverviewQuery(),
            cancellationToken);

        return result.Match(
            response => response,
            _ => throw new GraphQLException(
                "Unable to load admin dashboard statistics."));
    }

    [HotChocolate.Authorization.Authorize(Roles = [nameof(Role.Admin)])]
    public async Task<AdminMembersSummaryResponse> GetAdminMembersSummaryAsync(
    ISender sender,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminMembersSummaryQuery(),
            cancellationToken);

        return result.Match(
            response => response,
            _ => throw new GraphQLException(
                "Unable to load members summary."));
    }

    [HotChocolate.Authorization.Authorize(Roles = [nameof(Role.Admin)])]
    public async Task<AdminSubscriptionsSummaryResponse> GetAdminSubscriptionsSummaryAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminSubscriptionsSummaryQuery(),
            cancellationToken);

        return result.Match(
            response => response,
            _ => throw new GraphQLException(
                "Unable to load subscriptions summary."));
    }

    [HotChocolate.Authorization.Authorize(Roles = [nameof(Role.Admin)])]
    public async Task<AdminPaymentsSummaryResponse> GetAdminPaymentsSummaryAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminPaymentsSummaryQuery(),
            cancellationToken);

        return result.Match(
            response => response,
            _ => throw new GraphQLException(
                "Unable to load payments summary."));
    }

    [HotChocolate.Authorization.Authorize(Roles = [nameof(Role.Admin)])]
    public async Task<AdminPlansSummaryResponse> GetAdminPlansSummaryAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminPlansSummaryQuery(),
            cancellationToken);

        return result.Match(
            response => response,
            _ => throw new GraphQLException(
                "Unable to load plans summary."));
    }

    [HotChocolate.Authorization.Authorize(Roles = [nameof(Role.Admin)])]
    public async Task<AdminRecentCheckInsResponse> GetAdminRecentCheckInsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminRecentCheckInsQuery(),
            cancellationToken);

        return result.Match(
            response => response,
            _ => throw new GraphQLException(
                "Unable to load recent check-ins."));
    }

    [HotChocolate.Authorization.Authorize(Roles = [nameof(Role.Admin)])]
    public async Task<AdminRevenueSummaryResponse> GetAdminRevenueSummaryAsync(
        RevenuePeriod period,
        int? year,
        int? month,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminRevenueSummaryQuery(period, year, month),
            cancellationToken);

        return result.Match(
            response => response,
            _ => throw new GraphQLException(
                "Unable to load revenue summary."));
    }

    [HotChocolate.Authorization.Authorize(Roles = [nameof(Role.Admin)])]
    public async Task<AdminAttendanceSummaryResponse> GetAdminAttendanceSummaryAsync(
        AttendancePeriod period,
        int? year,
        int? month,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetAdminAttendanceSummaryQuery(period, year, month),
            cancellationToken);

        return result.Match(
            response => response,
            _ => throw new GraphQLException(
                "Unable to load attendance summary."));
    }
}