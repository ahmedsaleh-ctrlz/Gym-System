using Asp.Versioning;

using Gym.Api.Contracts.PromoCodes;
using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.PromoCodes.Commands.ActivatePromoCode;
using Gym.Application.Features.PromoCodes.Commands.CreatePromoCodeCommand;
using Gym.Application.Features.PromoCodes.Commands.DeactivatePromoCode;
using Gym.Application.Features.PromoCodes.Dtos;
using Gym.Application.Features.PromoCodes.Queries.GetByCode;
using Gym.Application.Features.PromoCodes.Queries.GetPromoCodeByIdQuery;
using Gym.Application.Features.PromoCodes.Queries.GetPromoCodesQuery;
using Gym.Domain.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gym.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/promo-codes")]
[ApiVersion("1.0")]
[Authorize]
public sealed class PromoCodesController(
    ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromoCodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves all promo codes.")]
    [EndpointDescription("Returns all promo codes with optional active status filtering and search.")]
    [EndpointName("GetPromoCodes")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetPromoCodes(
        CancellationToken ct,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null)
    {
        var result = await sender.Send(
            new GetPromoCodesQuery(
                isActive,
                search),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PromoCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a promo code by id.")]
    [EndpointDescription("Returns the specified promo code.")]
    [EndpointName("GetPromoCodeById")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetPromoCodeByIdQuery(id),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost("promo-preview")]
    [ProducesResponseType(typeof(PromoCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a promo code preview by code.")]
    [EndpointDescription("Returns the preview for promo code.")]
    [EndpointName("GetPromoCodePreview")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Preview(
        [FromBody]
        PreviewPromoCodeRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetPromoCodeByCodeQuery(request.PromoCode, request.MemberId, request.PlanId),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new promo code.")]
    [EndpointDescription("Creates a new promo code and notifies eligible members.")]
    [EndpointName("CreatePromoCode")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create(
        [FromBody] CreatePromoCodeRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreatePromoCodeCommand(
            request.Code, request.PlanId, request.DiscountType, request.DiscountValue, request.MaxDiscount,
            request.MinimumPurchaseAmount, request.Audience, request.UsageLimit, request.ExpiresAtUtc), ct);

        return result.Match(
            _ => StatusCode(StatusCodes.Status201Created),
            Problem);
    }

    [HttpPatch("{id:int}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Activates a promo code.")]
    [EndpointDescription("Activates the specified promo code.")]
    [EndpointName("ActivatePromoCode")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Activate(
        int id,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new ActivatePromoCodeCommand(id),
            ct);

        return result.Match(
            _ => Ok(),
            Problem);
    }

    [HttpPatch("{id:int}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Deactivates a promo code.")]
    [EndpointDescription("Deactivates the specified promo code.")]
    [EndpointName("DeactivatePromoCode")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new DeactivatePromoCodeCommand(id),
            ct);

        return result.Match(
            _ => Ok(),
            Problem);
    }
}