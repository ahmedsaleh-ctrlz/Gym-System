using System.Security.Claims;
using System.Text.RegularExpressions;

using Asp.Versioning;

using Gym.Api.Contracts.Identity;
using Gym.Application.Features.Identity.Commands.CompleteGoogleLogin;
using Gym.Application.Features.Identity.Commands.RegisterGoogleMember;
using Gym.Application.Features.Identity.Dtos;
using Gym.Infrastructure.Identity;

using MediatR;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace Gym.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth/external")]
public sealed class ExternalAuthController(
    SignInManager<AppUser> signInManager,
    ISender sender) : ApiController
{
    private const string FrontendCallbackUrl =
        "https://localhost:4200/oauth-google";

    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult GoogleLogin()
    {
        var callbackUrl = Url.Action(
            nameof(GoogleCallback),
            "ExternalAuth",
            new { version = "1.0" },
            Request.Scheme);

        var properties =
            signInManager.ConfigureExternalAuthenticationProperties(
                "Google",
                callbackUrl);

        return Challenge(properties, "Google");
    }

    [HttpGet("google-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(
    [FromQuery] string? remoteError)
    {
        if (!string.IsNullOrWhiteSpace(remoteError))
        {
            return Redirect(
                QueryHelpers.AddQueryString(
                    FrontendCallbackUrl,
                    "error",
                    "google_auth_failed"));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info is null || info.LoginProvider != "Google")
        {
            return Redirect(
                QueryHelpers.AddQueryString(
                    FrontendCallbackUrl,
                    "error",
                    "external_login_info_missing"));
        }

        return Redirect(
            QueryHelpers.AddQueryString(
                FrontendCallbackUrl,
                "status",
                "complete_google_login"));
    }


    [HttpPost("google/complete")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginRateLimiter")]
    public async Task<IActionResult> CompleteGoogleLogin(
    CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info is null || info.LoginProvider != "Google")
        {
            return Unauthorized(new
            {
                status = "SessionExpired",
                message = "Google login session expired. Please sign in again."
            });
        }

        var email =
            info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue("email");

        var emailVerified = bool.TryParse(
            info.Principal.FindFirstValue("email_verified"),
            out var verified) && verified;

        var result = await sender.Send(
            new CompleteGoogleLoginCommand(
                info.ProviderKey,
                email,
                emailVerified),
            ct);

        if (result.IsError)
        {
            return Problem(result.Errors);
        }

        var outcome = result.Value;

        switch (outcome.Status)
        {
            case GoogleLoginStatus.Authenticated:
                {
                    await HttpContext.SignOutAsync(
                        IdentityConstants.ExternalScheme);

                    return Ok(outcome);
                }

            case GoogleLoginStatus.RegistrationRequired:
                {
                    return Ok(outcome);
                }

            case GoogleLoginStatus.EmailVerificationRequired:
                {
                    await HttpContext.SignOutAsync(
                        IdentityConstants.ExternalScheme);

                    return Ok(outcome);
                }

            default:
                {
                    await HttpContext.SignOutAsync(
                        IdentityConstants.ExternalScheme);

                    return Problem("Unsupported Google login result.");
                }
        }
    }

    [HttpPost("google/register")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginRateLimiter")]
    public async Task<IActionResult> RegisterGoogle(
    [FromBody] RegisterGoogleMemberRequest request,
    CancellationToken ct)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info is null || info.LoginProvider != "Google")
        {
            return Unauthorized(
                "Google registration session expired. Please sign in again.");
        }

        var email =
            info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue("email");

        var emailVerified = bool.TryParse(
            info.Principal.FindFirstValue("email_verified"),
            out var verified) && verified;

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Google did not provide an email.");
        }

        var result = await sender.Send(
            new RegisterGoogleMemberCommand(
                info.ProviderKey,
                email,
                emailVerified,
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.PhoneNumber,
                request.ImageUrl),
            ct);

        var response = result.Match(
            tokens => (IActionResult)Ok(tokens),
            Problem);

        if (result.IsError)
        {
            return response;
        }

        await HttpContext.SignOutAsync(
            IdentityConstants.ExternalScheme);

        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        return response;
    }
}