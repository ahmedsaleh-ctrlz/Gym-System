using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Members.Commands.UpdatePassword;

public sealed class UpdatePasswordCommandHandler(IIdentityService identityService, IAppDbContext context, ILogger<UpdatePasswordCommandHandler> logger) : IRequestHandler<UpdatePasswordCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdatePasswordCommand command, CancellationToken ct)
    {
        logger.LogTrace("Handling Update Password command for Member ID {MemberId}.", command.MemberId);

        var memberResult = await context.Members.FindAsync(new object[] { command.MemberId }, ct);

        if (memberResult is null)
        {
            logger.LogWarning("Member with ID {MemberId} not found for password update.", command.MemberId);
            return ApplicationErrors.MemberNotFound;
        }

        var updateResult = await identityService.UpdatePasswordAsync(memberResult.PersonId, command.CurrentPassword, command.NewPassword);

        if (updateResult.IsError)
        {
            logger.LogWarning("Failed to update password for Member with ID {MemberId}. Errors: {Errors}", command.MemberId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return updateResult.Errors;
        }

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Successfully updated password for Member with ID {MemberId}.", command.MemberId);

        return Result.Updated;
    }
}
