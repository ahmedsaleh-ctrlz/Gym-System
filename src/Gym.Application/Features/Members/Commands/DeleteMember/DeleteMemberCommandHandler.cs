using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Members.Commands.DeleteMember;

public sealed class DeleteMemberCommandHandler(IAppDbContext context,
    ILogger<DeleteMemberCommandHandler> logger,
    IIdentityService identityService,
    HybridCache cache) : IRequestHandler<DeleteMemberCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteMemberCommand command , CancellationToken cancellationToken)
    {
        logger.LogTrace("Handling {CommandName} for MemberId: {MemberId}", nameof(DeleteMemberCommand), command.MemberId);
        var member = await context.Members.FindAsync([command.MemberId], cancellationToken);
        if (member is null)
        {
            logger.LogWarning("Member with id {MemberId} not found.", command.MemberId);
            return ApplicationErrors.MemberNotFound;
        }

        //var isSubscribed = await context.Subscriptions.Include(s => s.Member).
        //    AnyAsync(s => s.Member.Id == command.MemberId, cancellationToken);

        //if (isSubscribed)
        //{
        //    logger.LogWarning("Cannot delete member with id {MemberId} because they are subscribed.", command.MemberId);
        //    return ApplicationErrors.CannotDeleteSubscribedMember;
        //}

        var deleteResult = member!.Delete();
        if (deleteResult.IsError)
        {
            logger.LogError("Failed to delete member with id {MemberId}. Errors: {Errors}", command.MemberId, deleteResult.Errors);
            return deleteResult.Errors;
        }

        await identityService.DeleteUserAsync(member.PersonId,cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Removing cache with tag: Member");
        await cache.RemoveByTagAsync("Member", cancellationToken);

        logger.LogInformation("Member with id {MemberId} deleted successfully.", command.MemberId);
        return Result.Deleted;
    }
}
