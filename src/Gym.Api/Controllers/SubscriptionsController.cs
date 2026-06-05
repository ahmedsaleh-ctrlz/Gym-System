using Asp.Versioning;
using Gym.Api.Contracts.Subscriptions;
using Gym.Application.Common.Models;
using Gym.Application.Features.Subscriptions.Commands.CreateSubscription;
using Gym.Application.Features.Subscriptions.Commands.FreezeSubscription;
using Gym.Application.Features.Subscriptions.Commands.RenewSubscription;
using Gym.Application.Features.Subscriptions.Commands.UpdateSubscriptionStatus;
using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Application.Features.Subscriptions.Queries.GetMemberSubscriptions;
using Gym.Application.Features.Subscriptions.Queries.GetSubscriptionById;
using Gym.Application.Features.Subscriptions.Queries.GetSubscriptions;
using Gym.Domain.Identity;
using Gym.Domain.Subscriptions.Enums;
using Gym.Infrastructure.Identity.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/subscriptions")]
[ApiVersion("1.0")]
[Authorize]
public sealed class SubscriptionsController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<SubscriptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list of subscriptions.")]
    [EndpointDescription("Returns paged subscriptions.")]
    [EndpointName("GetSubscriptions")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Roles =$"{nameof(Role.Admin)},{nameof(Role.Coach)}")]
    public async Task<IActionResult> GetSubscriptions(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? planName = null,
        [FromQuery] DateOnly? startDateFrom = null,
        [FromQuery] DateOnly? startDateTo = null,
        [FromQuery] DateOnly? endDateFrom = null,
        [FromQuery] DateOnly? endDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        SubscriptionStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<SubscriptionStatus>(status, true, out var statusValue))
        {
            parsedStatus = statusValue;
        }

        var result = await sender.Send(
            new GetSubscriptionsQuery(
                pageNumber,
                pageSize,
                searchTerm,
                parsedStatus,
                planName,
                startDateFrom,
                startDateTo,
                endDateFrom,
                endDateTo,
                sortBy,
                sortDirection),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }


    [HttpGet("member/{id:int}/history")]
    [ProducesResponseType(typeof(SubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a subscription history by member ID.")]
    [EndpointDescription("Returns all subscription for the specified member if found.")]
    [EndpointName("GetSubscriptionHistoryByMember")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = Policies.SameMemberOrCoachOrAdmin)]
    public async Task<IActionResult> GetSubscriptionHistroyByMemberId(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetMemberSubscriptionsQuery(id), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }


    [HttpGet("member/{id:int}")]
    [ProducesResponseType(typeof(SubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a subscription by member ID.")]
    [EndpointDescription("Returns the latest subscription for the specified member if found.")]
    [EndpointName("GetSubscriptionByMemberId")]
    [MapToApiVersion("1.0")]
    [Authorize(Policy = Policies.SameMemberOrCoachOrAdmin)]
    public async Task<IActionResult> GetSubscriptionByMemberId(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetSubscriptionByMemberIdQuery(id), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Add subscription.")]
    [EndpointDescription("Add subscription and return the new route.")]
    [EndpointName("AddSubscription")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateSubscriptionCommand(request.MemberId, request.PlanId, request.StartDate),
            ct);

        return result.Match(
            _ => Created(),
            Problem);
    }

    [HttpPost("renew")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Renew subscription.")]
    [EndpointDescription("Renew subscription and return the new route.")]
    [EndpointName("RenewSubscription")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]

    public async Task<IActionResult> Renew([FromBody] RenewSubscriptionRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new RenewSubscriptionCommand(request.MemberId, request.PlanId),  
            ct);

        return result.Match(
            _ => Created(),
            Problem);
    }



    [HttpPut("{subscriptionId:int}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Activate a subscription.")]
    [EndpointDescription("Activates a subscription.")]
    [EndpointName("ActivateSubscription")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Activate(
        int subscriptionId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateSubscriptionStatusCommand(subscriptionId, SubscriptionStatus.Active),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPut("{subscriptionId:int}/schedule")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Schedule a subscription.")]
    [EndpointDescription("Schedules a subscription.")]
    [EndpointName("ScheduleSubscription")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Schedule(
        int subscriptionId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateSubscriptionStatusCommand(subscriptionId, SubscriptionStatus.Scheduled),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }


    [HttpPut("{subscriptionId:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Cancel a subscription.")]
    [EndpointDescription("Cancels a subscription.")]
    [EndpointName("CancelSubscription")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Cancel(
        int subscriptionId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateSubscriptionStatusCommand(subscriptionId, SubscriptionStatus.Cancelled),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPut("{subscriptionId:int}/freeze")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Freeze a subscription.")]
    [EndpointDescription("Freezes a subscription for the requested number of days.")]
    [EndpointName("FreezeSubscription")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Freeze(
        int subscriptionId,
        [FromBody] FreezeSubscriptionRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new FreezeSubscriptionCommand(subscriptionId, request.FreezeDays),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }
}
