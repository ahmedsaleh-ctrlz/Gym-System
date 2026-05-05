using Gym.Application.Common.Errors;
using Gym.Application.Common.Helpers;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Members;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Coaches.Commands.DeleteCoach;

public sealed class DeleteCoachCommandHandler(IAppDbContext context,
    ILogger<DeleteCoachCommandHandler> logger,
    IIdentityService identityService,
    HybridCache cache) : IRequestHandler<DeleteCoachCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteCoachCommand command , CancellationToken cancellationToken)
    {
        logger.LogTrace("Handling {CommandName} for CoachId: {CoachId}", nameof(DeleteCoachCommand), command.CoachId);
        var coach = await context.Coaches.Include(m => m.Person).ThenInclude(p => p.Image).FirstOrDefaultAsync(c => c.Id == command.CoachId, cancellationToken);
        if (coach is null)
        {
            logger.LogWarning("Member with id {CoachId} not found.", command.CoachId);
            return ApplicationErrors.CoachNotFound;
        }

        var deleteResult = coach.Deactivate();
        if (deleteResult.IsError)
        {
            logger.LogError("Failed to DeActivate Coach with id {CoachId}. Errors: {Errors}", command.CoachId, deleteResult.Errors);
            return deleteResult.Errors;
        }

        await identityService.DeleteUserAsync(coach.PersonId,cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrEmpty(coach.Person?.Image?.ImageUrl))
        {
            await Utility.DeleteImage(coach.Person.Image.ImageUrl);
        }
        await cache.RemoveByTagAsync("Coach", cancellationToken);

        logger.LogInformation("Member with id {CoachId} deleted successfully.", command.CoachId);
        return Result.Deleted;
    }
}
