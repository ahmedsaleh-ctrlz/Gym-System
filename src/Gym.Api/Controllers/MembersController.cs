using Asp.Versioning;

using Gym.Api.Contracts.Members;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Application.Features.Members.Commands.DeleteMember;
using Gym.Application.Features.Members.Commands.UpdateMember;
using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Members.Queries.GetCurrentMember;
using Gym.Application.Features.Members.Queries.GetMemberById;
using Gym.Application.Features.Members.Queries.GetMembers;
using Gym.Application.Features.Members.Queries.GetMembersWithActiveSubscription;
using Gym.Domain.Identity;
using Gym.Infrastructure.Identity.Policies;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/members")]
[ApiVersion("1.0")]
[Authorize]

public sealed class MembersController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<MemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list of members.")]
    [EndpointDescription("Returns Paged members.")]
    [EndpointName("GetMembers")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Coach)}")]

    public async Task<IActionResult> GetMembers(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var result = await sender.Send(new GetMembersQuery(pageNumber, pageSize, searchTerm, sortBy, sortDirection), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(List<ActiveMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list of members with active subscriptions.")]
    [EndpointDescription("Returns members with active subscriptions.")]
    [EndpointName("GetMembersWithActiveSubscription")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Coach)}")]

    public async Task<IActionResult> GetMembersWithActiveSubscription(CancellationToken ct)
    {
        var result = await sender.Send(new GetMembersWithActiveSubscriptionQuery(), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves the current logged-in member.")]
    [EndpointDescription("Returns detailed information about the current logged-in member.")]
    [EndpointName("GetCurrentMember")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Member))]

    public async Task<IActionResult> GetCurrentMember(CancellationToken ct)
    {
        var result = await sender.Send(new GetCurrentMemberQuery(), ct);
        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a member by ID.")]
    [EndpointDescription("Returns detailed information about the specified member if found.")]
    [EndpointName("GetMemberById")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = Policies.SameMemberOrCoachOrAdmin)]

    public async Task<IActionResult> GetMemberById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetMemberByIdQuery(id), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Add member.")]
    [EndpointDescription("Add member and return the new route.")]
    [EndpointName("AddMember")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var result = await sender.Send(new CreateMemberCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber,
            request.ImageUrl,
            request.JoinDate,
            request.Notes,
            request.Email,
            request.Password));

        return result.Match(
            _ => Created(),
            Problem);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update a member Information.")]
    [EndpointDescription("Update a member Information.")]
    [EndpointName("UpdateMember")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = Policies.SameMemberOrAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateMemberCommand(
            id,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber,
            request.JoinDate,
            request.Notes), ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPut("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update a memberImage Information.")]
    [EndpointDescription("Update a memberImage Information.")]
    [EndpointName("UpdateMemberImage")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = Policies.SameMemberOrAdmin)]

    public async Task<IActionResult> UpdateImage(int id, [FromBody] UpdateMemberImageRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateMemberImageCommand(id, request.ImageUrl), ct);
        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes a member.")]
    [EndpointDescription("Deletes the specified member from the system.")]
    [EndpointName("RemoveMember")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteMemberCommand(id), ct);
        return result.Match(
            _ => NoContent(),
            Problem);
    }
}
