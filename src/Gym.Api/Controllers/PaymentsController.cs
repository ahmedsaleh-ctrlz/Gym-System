using Asp.Versioning;
using Gym.Api.Contracts.Payments;
using Gym.Application.Common.Models;
using Gym.Application.Features.Payments.Commands.PayPayment;
using Gym.Application.Features.Payments.Dtos;
using Gym.Application.Features.Payments.Queries.GetPaymentById;
using Gym.Application.Features.Payments.Queries.GetPayments;
using Gym.Domain.Identity;
using Gym.Domain.Payments.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/payments")]
[ApiVersion("1.0")]
[Authorize]
public sealed class PaymentsController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a list of payments.")]
    [EndpointDescription("Returns paged payments.")]
    [EndpointName("GetPayments")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> GetPayments(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? memberId = null,
        [FromQuery] int? subscriptionId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        PaymentStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<PaymentStatus>(status, true, out var statusValue))
        {
            parsedStatus = statusValue;
        }

        var result = await sender.Send(
            new GetPaymentsQuery(pageNumber, pageSize, searchTerm, memberId, subscriptionId, parsedStatus, sortBy, sortDirection),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a payment by ID.")]
    [EndpointDescription("Returns detailed information about the specified payment if found.")]
    [EndpointName("GetPaymentById")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> GetPaymentById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPaymentByIdQuery(id), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost("Pay")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Record a payment.")]
    [EndpointDescription("Records a successful or failed payment for a subscription.")]
    [EndpointName("PayPayment")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create([FromBody] PayPaymentRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new PayPaymentCommand(request.PaymentId, request.PaymentMethod),
            ct);

        return result.Match(
            _ => Created(),
            Problem);
    }
}
