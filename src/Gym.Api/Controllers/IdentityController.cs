using Asp.Versioning;
using Gym.Api.Contracts.Identity;
using Gym.Api.Contracts.Members;
using Gym.Application.Features.Identity.Dtos;
using Gym.Application.Features.Identity.Queries.GenerateToken;
using Gym.Application.Features.Identity.Queries.RefreshToken;
using Gym.Application.Features.Members.Commands.CreateMember;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers
{
    [Route("api/v{version:apiVersion}/Identity")]
    [ApiController]
    [ApiVersion("1.0")]
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
            var result = await sender.Send(new GenerateTokenQuery(request.email, request.password), ct);

            return result.Match(
                response => Ok(response)
                , Problem);
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
            var result = await sender.Send(new RefreshTokenQuery(request.RefreshToken,request.ExpiredAccessToken), ct);
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
        [EndpointName("RegisterMember")]
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
               _ => Created()
                    , Problem);
        }
    }
}
