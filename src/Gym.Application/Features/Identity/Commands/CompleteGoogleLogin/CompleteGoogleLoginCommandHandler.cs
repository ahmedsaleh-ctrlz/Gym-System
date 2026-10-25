using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Identity.Commands.CompleteGoogleLogin;

public sealed class CompleteGoogleLoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider)
    : IRequestHandler<
        CompleteGoogleLoginCommand,
        Result<GoogleLoginOutcome>>
{
    public async Task<Result<GoogleLoginOutcome>> Handle(
        CompleteGoogleLoginCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.ProviderKey))
        {
            return Error.Validation(
                "Invalid_Google_Login",
                "Google login information is invalid.");
        }

        var linkedUser = await identityService.FindByExternalLoginAsync(
            "Google",
            command.ProviderKey,
            cancellationToken);

        if (linkedUser is not null)
        {
            return await AuthenticateAsync(linkedUser, cancellationToken);
        }

        if (!command.EmailVerified ||
            string.IsNullOrWhiteSpace(command.Email))
        {
            return new GoogleLoginOutcome(
                GoogleLoginStatus.EmailVerificationRequired);
        }

        var existingUser = await identityService.FindUserByEmailAsync(
            command.Email,
            cancellationToken);

        if (existingUser is not null)
        {
            var linkResult = await identityService.LinkExternalLoginAsync(
                existingUser.UserId,
                "Google",
                command.ProviderKey,
                command.EmailVerified,
                cancellationToken);

            if (linkResult.IsError)
            {
                return linkResult.Errors;
            }

            return await AuthenticateAsync(
                linkResult.Value,
                cancellationToken);
        }

        return new GoogleLoginOutcome(
            GoogleLoginStatus.RegistrationRequired);
    }

    private async Task<Result<GoogleLoginOutcome>> AuthenticateAsync(
        AppUserDto user,
        CancellationToken ct)
    {
        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(user, ct);

        if (tokenResult.IsError)
        {
            return tokenResult.Errors;
        }

        return new GoogleLoginOutcome(
            GoogleLoginStatus.Authenticated,
            tokenResult.Value);
    }
}