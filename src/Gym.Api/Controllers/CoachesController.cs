using Asp.Versioning;
using Gym.Api.Contracts.Coaches;
using Gym.Application.Features.Coaches.Commands.CreateCoach;
using Gym.Application.Features.Coaches.Commands.DeleteCoach;
using Gym.Application.Features.Coaches.Commands.UpdateCoach;
using Gym.Application.Features.Coaches.Dtos;
using Gym.Application.Features.Coaches.Queries.GetCoachById;
using Gym.Application.Features.Coaches.Queries.GetCoaches;
using Gym.Domain.Identity;
using Gym.Infrastructure.Identity.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Cryptography;

namespace Gym.Api.Controllers;


[ApiController]
[Route("api/v{version:apiVersion}/coaches")]
[ApiVersion("1.0")]
[Authorize]
public sealed class CoachesController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<CoachResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list of Coachs.")]
    [EndpointDescription("Returns Paged Coachs.")]
    [EndpointName("GetCoachs")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [AllowAnonymous]
    public async Task<IActionResult> GetCoachs(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var result = await sender.Send(new GetCoachesQuery(pageNumber, pageSize, searchTerm, sortBy, sortDirection), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CoachResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a Coach by ID.")]
    [EndpointDescription("Returns detailed information about the specified Coach if found.")]
    [EndpointName("GetCoachById")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = Policies.SameCoach)]

    public async Task<IActionResult> GetCoachById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetCoachByIdQuery(id), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Add Coach.")]
    [EndpointDescription("Add Coach and return the new route.")]
    [EndpointName("AddCoach")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateCoachRequest request)
    {
        var result = await sender.Send(new CreateCoachCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber,
            request.ImageUrl,
            request.HireDate,
            request.Email,
            request.Password));

        return result.Match(
           _ => Created()
                , Problem);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update a Coach Information.")]
    [EndpointDescription("Update a Coach Information.")]
    [EndpointName("UpdateCoach")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCoachRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCoachCommand(
            id,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber,
            request.HireDate), ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPut("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update a CoachImage Information.")]
    [EndpointDescription("Update a CoachImage Information.")]
    [EndpointName("UpdateCoachImage")]
    [MapToApiVersion("1.0")]

    public async Task<IActionResult> UpdateImage(int id, [FromBody] UpdateCoachImageRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCoachImageCommand(id, request.ImageUrl), ct);
        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes a Coach.")]
    [EndpointDescription("Deletes the specified Coach from the system.")]
    [EndpointName("RemoveCoach")]
    [MapToApiVersion("1.0")]

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteCoachCommand(id), ct);
        return result.Match(
            _ => NoContent(),
            Problem);
    }
}
