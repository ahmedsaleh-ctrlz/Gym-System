using Asp.Versioning;

using Gym.Api.Contracts.Plans;
using Gym.Application.Common.Models;
using Gym.Application.Features.Plans.Commands;
using Gym.Application.Features.Plans.Commands.CreatePlan;
using Gym.Application.Features.Plans.Commands.DeletePlan;
using Gym.Application.Features.Plans.Commands.UpdatePlan;
using Gym.Application.Features.Plans.Dtos;
using Gym.Application.Features.Plans.Queries.GetPlanById;
using Gym.Application.Features.Plans.Queries.GetPlans;
using Gym.Domain.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/plans")]
[ApiVersion("1.0")]
[Authorize]
public sealed class PlansController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<PlanDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list of plans.")]
    [EndpointDescription("Returns paged plans.")]
    [EndpointName("GetPlans")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var result = await sender.Send(
            new GetPlansQuery(pageNumber, pageSize, searchTerm, sortBy, sortDirection),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlanDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a plan by ID.")]
    [EndpointDescription("Returns detailed information about the specified plan if found.")]
    [EndpointName("GetPlanById")]
    [MapToApiVersion("1.0")]

    public async Task<IActionResult> GetPlanById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPlanByIdQuery(id), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Add plan.")]
    [EndpointDescription("Add plan and return the new route.")]
    [EndpointName("AddPlan")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create([FromBody] CreatePlanRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new CreatePlanCommand(
            request.Title,
            request.Description,
            request.Cost,
            request.DurationInDays,
            request.AllowedFreezeCount,
            request.MaxTotalFreezeDays), ct);
        return result.Match(
            _ => Created(),
            Problem);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Update a plan information.")]
    [EndpointDescription("Update a plan information.")]
    [EndpointName("UpdatePlan")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlanRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePlanCommand(
            id,
            request.Title,
            request.Description,
            request.Cost,
            request.DurationInDays,
            request.AllowedFreezeCount,
            request.MaxTotalFreezeDays), ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes a plan.")]
    [EndpointDescription("Deactivates the specified plan from the system.")]
    [EndpointName("RemovePlan")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeletePlanCommand(id), ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }
}