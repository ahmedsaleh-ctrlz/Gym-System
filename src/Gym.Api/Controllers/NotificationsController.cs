using Asp.Versioning;

using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Notifications.Commands;
using Gym.Application.Features.Notifications.Dtos;

using Gym.Application.Features.Notifications.Queries.GetNotificationsQuery;
using Gym.Application.Features.Notifications.Queries.GetUnreadCountQuery;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/notifications")]
[ApiVersion("1.0")]
[Authorize]
public sealed class NotificationsController(
    ISender sender,
    IUser currentUser) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves notifications for the current user.")]
    [EndpointDescription("Returns paged notifications for the authenticated user with optional read status filtering.")]
    [EndpointName("GetNotifications")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetNotifications(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null)
    {
        var result = await sender.Send(
            new GetNotificationsQuery(
                currentUser.Id!,
                pageNumber,
                pageSize,
                isRead),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves the unread notifications count.")]
    [EndpointDescription("Returns the number of unread notifications for the authenticated user.")]
    [EndpointName("GetUnreadNotificationsCount")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var result = await sender.Send(
            new GetUnreadCountQuery(currentUser.Id!),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPatch("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Marks a notification as read.")]
    [EndpointDescription("Marks the specified notification as read for the authenticated user.")]
    [EndpointName("MarkNotificationAsRead")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
    {
        var result = await sender.Send(
            new MarkAsReadCommand(
                id,
                currentUser.Id!),
            ct);

        return result.Match(
            _ => Ok(),
            Problem);
    }

    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Marks all notifications as read.")]
    [EndpointDescription("Marks all unread notifications for the authenticated user as read.")]
    [EndpointName("MarkAllNotificationsAsRead")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var result = await sender.Send(
            new MarkAllAsReadCommand(currentUser.Id!),
            ct);

        return result.Match(
            _ => Ok(),
            Problem);
    }
}