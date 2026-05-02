using Gym.Api.Contracts.Members;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Application.Features.Members.Commands.DeleteMember;
using Gym.Application.Features.Members.Commands.UpdateMember;
using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Members.Queries.GetMemberById;
using Gym.Application.Features.Members.Queries.GetMembers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/members")]
[ApiVersion("1.0")]
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
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> GetMembers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var result = await sender.Send(new GetMemberQuery(pageNumber, pageSize, searchTerm, sortBy, sortDirection));

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
    [OutputCache(Duration = 60)]

    public async Task<IActionResult> GetMemberById(int id , CancellationToken ct)
    {
        var result = await sender.Send(new GetMemberByIdQuery(id), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost]
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
           response => CreatedAtRoute(
                routeName: "GetMemberById",
                routeValues: new { memberId = response.MemberId },
                value: response),
            Problem); 
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberRequest request)
    {
        var result = await sender.Send(new UpdateMemberCommand(
            id,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber,
            request.JoinDate,
            request.Notes));

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPut("{id:int}/image")]
    public async Task<IActionResult> UpdateImage(int id, [FromBody] UpdateMemberImageRequest request)
    {
        var result = await sender.Send(new UpdateMemberImageCommand(id, request.ImageUrl));
        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await sender.Send(new DeleteMemberCommand(id));
        return result.Match(
            _ => NoContent(),
            Problem);
    }

    
}
