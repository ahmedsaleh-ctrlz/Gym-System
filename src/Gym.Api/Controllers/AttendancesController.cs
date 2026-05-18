using Asp.Versioning;
using Gym.Application.Common.Models;
using Gym.Application.Features.Attendances.Commands.CheckInMember;
using Gym.Application.Features.Attendances.Dtos;
using Gym.Application.Features.Attendances.Queries.GetAttendances;
using Gym.Application.Features.Attendances.Queries.GetMemberAttendanceHistory;
using Gym.Domain.Identity;
using Gym.Infrastructure.Identity.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/attendances")]
[ApiVersion("1.0")]
[Authorize]
public sealed class AttendancesController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AttendanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list of attendance records.")]
    [EndpointDescription("Returns paged attendance records.")]
    [EndpointName("GetAttendances")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> GetAttendances(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var result = await sender.Send(
            new GetAttendancesQuery(pageNumber, pageSize, searchTerm, dateFrom, dateTo, sortBy, sortDirection),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(PaginatedList<AttendanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves attendance history for a member.")]
    [EndpointDescription("Returns paged attendance history for the specified member.")]
    [EndpointName("GetMemberAttendanceHistory")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = Policies.SameMemberOrAdmin)]
    public async Task<IActionResult> GetMemberAttendanceHistory(
        int id,
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var result = await sender.Send(
            new GetMemberAttendanceHistoryQuery(id, pageNumber, pageSize, dateFrom, dateTo, sortDirection),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost("{id:int}/check-in")]
    [ProducesResponseType(typeof(AttendanceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Check-in a member.")]
    [EndpointDescription("Creates a timestamped attendance record for the specified member.")]
    [EndpointName("CheckInMember")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> CheckInMember(int id, CancellationToken ct)
    {
        var result = await sender.Send(new CheckInMemberCommand(id), ct);

        return result.Match(
            _ => Created(),
            Problem);
    }
}
