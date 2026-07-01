using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;
using Gym.Domain.Members;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Identity.Commands.RegisterMember;

public sealed class RegisterMemberCommandHandler(
    ILogger<RegisterMemberCommandHandler> logger,
    IAppDbContext context,
    IIdentityService identityService,
    IEmailSender emailSender)
    : IRequestHandler<RegisterMemberCommand, Result<Created>>
{
    public async Task<Result<Created>> Handle(
     RegisterMemberCommand request,
     CancellationToken ct)
    {
        logger.LogTrace(
            "Registering new member with email: {Email}",
            request.Email);

        await using var transaction =
            await context.Database.BeginTransactionAsync(ct);

        string? userId = null;
        int? personId = null;
        Member? member = null;
        string? confirmationUrl = null;

        try
        {
            var memberResult = Member.Create(
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.PhoneNumber,
                request.ImageUrl,
                DateTime.UtcNow,
                null);

            if (memberResult.IsError)
            {
                return memberResult.Errors;
            }

            member = memberResult.Value;

            await context.Members.AddAsync(member, ct);
            await context.SaveChangesAsync(ct);

            personId = member.Person.Id;

            var userResult = await identityService.CreateUserAsync(
                request.Email,
                request.Password,
                Role.Member,
                personId.Value,
                ct);

            if (userResult.IsError)
            {
                logger.LogError(
                    "Failed to create user for member with email: {Email}. Errors: {Errors}",
                    request.Email,
                    userResult.Errors);

                await transaction.RollbackAsync(ct);
                return userResult.Errors;
            }

            userId = userResult.Value;

            var confirmationUrlResult =
                await identityService.GenerateEmailConfirmationUrlAsync(userId);

            if (confirmationUrlResult.IsError)
            {
                logger.LogError(
                    "Failed to generate email confirmation URL for user {UserId}. Errors: {Errors}",
                    userId,
                    confirmationUrlResult.Errors);

                await transaction.RollbackAsync(ct);
                return confirmationUrlResult.Errors;
            }

            confirmationUrl = confirmationUrlResult.Value;

            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            if (userId is not null)
            {
                await identityService.DeleteUserAsync(personId!.Value, ct);
            }

            logger.LogError(
                ex,
                "Error registering member with email {Email}",
                request.Email);

            throw;
        }

        try
        {
            await emailSender.SendEmailConfirmationAsync(
                request.Email,
                confirmationUrl!,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send confirmation email to {Email}",
                request.Email);
        }

        logger.LogInformation(
            "Successfully registered member {MemberId} with user {UserId}",
            member!.Id,
            userId);

        return Result.Created;
    }
}