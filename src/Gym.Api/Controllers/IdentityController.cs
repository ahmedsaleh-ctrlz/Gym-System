using Asp.Versioning;

using Gym.Api.Contracts.Identity;
using Gym.Api.Contracts.Members;
using Gym.Application.Features.Identity.Commands.ConfirmEmail;
using Gym.Application.Features.Identity.Commands.RegisterMember;
using Gym.Application.Features.Identity.Commands.ResendConfirmationEmail;
using Gym.Application.Features.Identity.Dtos;
using Gym.Application.Features.Identity.Queries.GenerateToken;
using Gym.Application.Features.Identity.Queries.RefreshToken;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Application.Features.Members.Commands.UpdatePassword;
using Gym.Infrastructure.Identity.Policies;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers
{
    [Route("api/v{version:apiVersion}/Identity")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class IdentityController(ISender sender) : ApiController
    {
        [HttpPost("token/generate")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Generates an access and refresh token for a valid user.")]
        [EndpointDescription("Authenticates a user using provided credentials and returns a JWT token pair.")]
        [EndpointName("GenerateToken")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<TokenResponse>> GenerateToken([FromBody] GenerateTokenRequest request, CancellationToken ct)
        {
            var result = await sender.Send(new GenerateTokenQuery(request.Email, request.Password), ct);

            return result.Match(
                response => Ok(response),
                Problem);
        }

        [HttpPost("token/refresh-token")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Refreshes access token using a valid refresh token.")]
        [EndpointDescription("Exchanges an expired access token and a valid refresh token for a new token pair.")]
        [EndpointName("RefreshToken")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await sender.Send(new RefreshTokenQuery(request.RefreshToken, request.ExpiredAccessToken), ct);
            return result.Match(
                response => Ok(response),
                Problem);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Register.")]
        [EndpointDescription("Registers a new member and returns the new route.")]
        [EndpointName("RegisterMemberV1")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Register([FromBody] CreateMemberRequest request)
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Register.")]
        [EndpointDescription("Registers a new member and returns the new route.")]
        [EndpointName("RegisterMemberV2")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> RegisterV2([FromBody] RegisterMemberRequest request)
        {
            var result = await sender.Send(new RegisterMemberCommand(
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.PhoneNumber,
                request.ImageUrl,
                request.Email,
                request.Password));

            return result.Match(
                _ => Created(),
                Problem);
        }

        [HttpPut("update-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Update a member's password.")]
        [EndpointDescription("Update a member's password.")]
        [EndpointName("UpdateMemberPassword")]
        [MapToApiVersion("1.0")]
        [Authorize(Policy = Policies.SameMemberOrAdmin)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateMemberPasswordRequest request, CancellationToken ct)
        {
            var result = await sender.Send(
                new UpdatePasswordCommand(
                    request.MemberId,
                    request.CurrentPassword,
                    request.NewPassword), ct);

            return result.Match(
                _ => NoContent(),
                Problem);
        }

        [HttpGet("confirm-email")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [EndpointSummary("Confirms a user's email.")]
        [EndpointDescription("Confirms a user's email using the confirmation token.")]
        [EndpointName("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token,CancellationToken ct)
        {
            var result = await sender.Send(
                new ConfirmEmailCommand(userId, token),
                ct);

            return result.Match(
                _ => Ok(new
                {
                    Message = "Email confirmed successfully."
                }),
                Problem);
        }

        [HttpPost("resend-confirmation-email")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [EndpointSummary("Resends the email confirmation link.")]
        [EndpointDescription("Resends the email confirmation link to the user's email address.")]
        [EndpointName("ResendConfirmationEmail")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequest request)
        {
            var result = await sender.Send(new ResendConfirmationCommand(request.Email));

            return result.Match(
                _ => Ok(new
                {
                    Message = "Confirmation email resent successfully."
                }),
                Problem);
        }
    }
}
