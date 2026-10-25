using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;
using Gym.Domain.Members;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Identity.Commands.RegisterGoogleMember;

public sealed class RegisterGoogleMemberCommandHandler(
    IAppDbContext context,
    ILogger<RegisterGoogleMemberCommandHandler> logger,
    HybridCache cache,
    IIdentityService identityService,
    IImageStorage imageStorage,
    ITokenProvider tokenProvider)
    : IRequestHandler<
        RegisterGoogleMemberCommand,
        Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(
        RegisterGoogleMemberCommand command,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.ProviderKey) ||
            string.IsNullOrWhiteSpace(command.Email))
        {
            return Error.Validation(
                "Invalid_Google_Login",
                "Google registration information is invalid.");
        }

        // Case 1: This Google account is already linked.
        var linkedUser = await identityService.FindByExternalLoginAsync(
            "Google",
            command.ProviderKey,
            ct);

        if (linkedUser is not null)
        {
            return await tokenProvider.GenerateJwtTokenAsync(
                linkedUser,
                ct);
        }

        // A new account must have a verified email before registration.
        if (!command.EmailVerified)
        {
            return Error.Validation(
                "Email_Not_Verified",
                "Google email must be verified before registration.");
        }

        // Case 2: The local account exists, but Google is not linked.
        var existingUser = await identityService.FindUserByEmailAsync(
            command.Email,
            ct);

        if (existingUser is not null)
        {
            var linkResult = await identityService.LinkExternalLoginAsync(
                existingUser.UserId,
                "Google",
                command.ProviderKey,
                command.EmailVerified,
                ct);

            if (linkResult.IsError)
            {
                return linkResult.Errors;
            }

            return await tokenProvider.GenerateJwtTokenAsync(
                linkResult.Value,
                ct);
        }

        // Case 3: Neither the Google login nor the local user exists.
        return await CreateNewGoogleMemberAsync(command, ct);
    }

    private async Task<Result<TokenResponse>> CreateNewGoogleMemberAsync(
        RegisterGoogleMemberCommand command,
        CancellationToken ct)
    {
        await using var transaction =
            await context.Database.BeginTransactionAsync(ct);

        const string defaultImageUrl = "images/default-image.png";

        var imageUrl = string.IsNullOrWhiteSpace(command.ImageUrl)
            ? defaultImageUrl
            : command.ImageUrl;

        string? promotedImageUrl = null;
        var wasPromoted = false;

        try
        {
            var memberResult = Member.Create(
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                command.PhoneNumber,
                imageUrl,
                DateTime.UtcNow,
                notes: null);

            if (memberResult.IsError)
            {
                await transaction.RollbackAsync(ct);
                return memberResult.Errors;
            }

            var member = memberResult.Value;

            await context.Members.AddAsync(member, ct);
            await context.SaveChangesAsync(ct);

            var userResult = await identityService.CreateExternalUserAsync(
                command.Email,
                Role.Member,
                member.Person.Id,
                ct);

            if (userResult.IsError)
            {
                await transaction.RollbackAsync(ct);
                return userResult.Errors;
            }

            var linkResult = await identityService.LinkExternalLoginAsync(
                userResult.Value,
                "Google",
                command.ProviderKey,
                command.EmailVerified,
                ct);

            if (linkResult.IsError)
            {
                await transaction.RollbackAsync(ct);
                return linkResult.Errors;
            }

            promotedImageUrl = await imageStorage.PromoteTemporaryAsync(
                imageUrl,
                ct);

            wasPromoted =
                !string.IsNullOrWhiteSpace(promotedImageUrl) &&
                !string.Equals(
                    promotedImageUrl,
                    imageUrl,
                    StringComparison.OrdinalIgnoreCase);

            var imageUpdateResult = member.UpdateImage(promotedImageUrl);

            if (imageUpdateResult.IsError)
            {
                await transaction.RollbackAsync(ct);

                if (wasPromoted)
                {
                    await imageStorage.DeleteAsync(promotedImageUrl!, ct);
                }

                return imageUpdateResult.Errors;
            }

            await context.SaveChangesAsync(ct);

            var tokenResult = await tokenProvider.GenerateJwtTokenAsync(
                linkResult.Value,
                ct);

            if (tokenResult.IsError)
            {
                await transaction.RollbackAsync(ct);

                if (wasPromoted)
                {
                    await imageStorage.DeleteAsync(promotedImageUrl!, ct);
                }

                return tokenResult.Errors;
            }

            await transaction.CommitAsync(ct);

            await cache.RemoveByTagAsync("AdminDashboard:Overview", ct);
            await cache.RemoveByTagAsync("AdminDashboard:MembersSummary", ct);
            await cache.RemoveByTagAsync("Member", ct);

            logger.LogInformation(
                "Registered Google member {MemberId} with user {UserId}",
                member.Id,
                userResult.Value);

            return tokenResult.Value;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            if (wasPromoted && !string.IsNullOrWhiteSpace(promotedImageUrl))
            {
                await imageStorage.DeleteAsync(promotedImageUrl, ct);
            }

            logger.LogError(
                ex,
                "Error registering Google member for {Email}",
                command.Email);

            throw;
        }
    }
}