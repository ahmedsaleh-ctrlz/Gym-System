using Asp.Versioning;

using Gym.Application.Features.Dashboard.Dtos;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.AttendanceSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.MembersSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Overview;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PaymentsSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PlansSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.RecentCheckIns;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.RevenueSummary;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Subscriptions_Summary;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/dashboard")]
public class DashBoardController(ISender sender) : ApiController
{
    [HttpGet("stats")]
    [ProducesResponseType(
        typeof(AdminDashboardOverviewResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result =
            await sender.Send(
                new GetAdminDashboardOverviewQuery(),
                ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("members-summary")]
    [ProducesResponseType(
        typeof(AdminMembersSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMembersSummary(CancellationToken ct)
    {
        var result =
            await sender.Send(
                new GetAdminMembersSummaryQuery(),
                ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("subscriptions-summary")]
    [ProducesResponseType(
        typeof(AdminSubscriptionsSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSubscriptionsSummary(CancellationToken ct)
    {
        var result =
            await sender.Send(
                new GetAdminSubscriptionsSummaryQuery(),
                ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("payments-summary")]
    [ProducesResponseType(
        typeof(AdminPaymentsSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPaymentsSummary(CancellationToken ct)
    {
        var result =
            await sender.Send(
                new GetAdminPaymentsSummaryQuery(),
                ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("revenue-summary")]
    [ProducesResponseType(
        typeof(AdminRevenueSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRevenueSummary(
        [FromQuery] RevenuePeriod period,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        var result =
            await sender.Send(
                new GetAdminRevenueSummaryQuery(
                    period,
                    year,
                    month),
                ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("attendance-summary")]
    [ProducesResponseType(
        typeof(AdminAttendanceSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAttendanceSummary(
        [FromQuery] AttendancePeriod period,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        var result =
            await sender.Send(
                new GetAdminAttendanceSummaryQuery(
                    period,
                    year,
                    month),
                ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("plans-summary")]
    [ProducesResponseType(
        typeof(AdminPlansSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPlansSummary(CancellationToken ct)
    {
        var result =
            await sender.Send(
                new GetAdminPlansSummaryQuery(),
                ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("recent-check-ins")]
    [ProducesResponseType(
        typeof(AdminRecentCheckInsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRecentCheckIns(CancellationToken ct)
    {
        var result =
            await sender.Send(
                new GetAdminRecentCheckInsQuery(),
                ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }
}