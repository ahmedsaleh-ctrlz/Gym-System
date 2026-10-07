using Asp.Versioning;

using Gym.Api.Contracts.Payments;
using Gym.Application.Common.Models;
using Gym.Application.Features.Payments.Commands.ApprovePayment;
using Gym.Application.Features.Payments.Commands.CancelPayment;
using Gym.Application.Features.Payments.Commands.CreateStripePayment;
using Gym.Application.Features.Payments.Commands.PayPayment;
using Gym.Application.Features.Payments.Commands.ProcessStripePayment;
using Gym.Application.Features.Payments.Commands.RefundPayment;
using Gym.Application.Features.Payments.Commands.RejectPayment;
using Gym.Application.Features.Payments.Commands.SubmitPaymentForReview;
using Gym.Application.Features.Payments.Dtos;
using Gym.Application.Features.Payments.Queries.GetMemberPayments;
using Gym.Application.Features.Payments.Queries.GetPaymentById;
using Gym.Application.Features.Payments.Queries.GetPayments;
using Gym.Domain.Identity;
using Gym.Domain.Payments.Enums;
using Gym.Infrastructure.Identity.Policies;
using Gym.Infrastructure.Settings;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Stripe;

namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/payments")]
[ApiVersion("1.0")]
[Authorize]
public sealed class PaymentsController(ISender sender, IOptions<StripeSettings> stripeOptions) : ApiController
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

    [HttpGet("member/{Id:int}")]
    [ProducesResponseType(typeof(List<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a payments history for a requested member.")]
    [EndpointDescription("Returns member payments history.")]
    [EndpointName("GetMemberPaymentsHistory")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Policy = Policies.SameMemberOrCoachOrAdmin)]

    public async Task<IActionResult> GetMemberPaymentsHistory(int Id, CancellationToken ct)
    {
        var response = await sender.Send(new GetMemberPaymentsQuery(Id), ct);
        return
            response.Match(response => Ok(response), Problem);
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Record a payment.")]
    [EndpointDescription("Records a successful or failed payment for a subscription.")]
    [EndpointName("PayPayment")]
    [MapToApiVersion("1.0")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] PayPaymentRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new PayPaymentCommand(request.PaymentId, request.PaymentMethod, request.PaymentReference, request.PromoCodeId!),
            ct);

        return result.Match(
            _ => Created(),
            Problem);
    }

    [HttpPost("refund")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Refund a payment.")]
    [EndpointDescription("Refund payment and cancel a subscription related with it.")]
    [EndpointName("RefundPayment")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Refund([FromBody] RefundPaymentRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new RefundPaymentCommand(request.PaymentId),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPut("Cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Cancel a payment.")]
    [EndpointDescription("Cancels a pending payment for a subscription.")]
    [EndpointName("CancelPayment")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Cancel([FromBody] CancelPaymentRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new CancelPaymentCommand(request.PaymentId),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("SubmitForReview")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Submit a payment for review.")]
    [EndpointDescription("Submits a manual payment made through E-Wallet or InstaPay for admin review.")]
    [EndpointName("SubmitPaymentForReview")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Member))]
    public async Task<IActionResult> SubmitForReview(
    [FromBody] SubmitPaymentForReviewRequest request,
    CancellationToken ct)
    {
        var result = await sender.Send(
            new SubmitPaymentForReviewCommand(
                request.PaymentId,
                request.PaymentMethod,
                request.PaymentReference,
                request.PromoCodeId),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("{paymentId:int}/Approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Approve a payment.")]
    [EndpointDescription("Approves a payment that is currently under review.")]
    [EndpointName("ApprovePayment")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Approve(
    int paymentId,
    CancellationToken ct)
    {
        var result = await sender.Send(
            new ApprovePaymentCommand(paymentId),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("{paymentId:int}/Reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Reject a payment.")]
    [EndpointDescription("Rejects a payment that is currently under review.")]
    [EndpointName("RejectPayment")]
    [MapToApiVersion("1.0")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Reject(
    int paymentId,
    CancellationToken ct)
    {
        var result = await sender.Send(
            new RejectPaymentCommand(paymentId),
            ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("{paymentId:int}/stripe-intent")]
    [ProducesResponseType(typeof(StripePaymentIntentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a Stripe payment intent.")]
    [EndpointDescription("Creates a Stripe payment intent for the specified pending payment and returns the client secret required for checkout.")]
    [EndpointName("CreateStripePaymentIntent")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> CreateStripePaymentIntent(
        CreateStripePaymentIntentRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateStripePaymentIntentCommand(request.PaymentId, request.PromoCodeId),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var stripeSettings = stripeOptions.Value;

        var json = await new StreamReader(Request.Body)
            .ReadToEndAsync();

        var stripeEvent = EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            stripeSettings.WebhookSecret);

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent =
                stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent is null)
            {
                return BadRequest();
            }

            int? promoCodeId = null;

            if (paymentIntent.Metadata.TryGetValue(
                    "promoCodeId",
                    out var value) &&
                int.TryParse(value, out var parsedPromoCodeId))
            {
                promoCodeId = parsedPromoCodeId;
            }

            await sender.Send(
                new ProcessStripePaymentCommand(
                    paymentIntent.Id,
                    promoCodeId),
                CancellationToken.None);
        }

        return Ok();
    }
}