using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Coachs;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Coaches.Commands.UpdateCoach;

public sealed class UpdateCoachCommandHandler(IAppDbContext context,
                                               ILogger<UpdateCoachCommandHandler> logger,
                                               HybridCache cache) : IRequestHandler<UpdateCoachCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UpdateCoachCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateCoachCommand command, CancellationToken ct)
    {
        _logger.LogTrace("Handling Update Member command for Coach ID {CoachId}.", command.CoachId);

        var coach = await _context.Coaches.Include(m => m.Person).FirstOrDefaultAsync(m => m.Id == command.CoachId, ct);

        if (coach is null)
        {
            _logger.LogWarning("Coach with ID {CoachId} not found for update.", command.CoachId);
            return ApplicationErrors.CoachNotFound;
        }

        var updateResult = coach.UpdateInfo(
            command.FirstName, command.LastName, command.DateOfBirth, command.PhoneNumber, command.HireDate);

        if (updateResult.IsError)
        {
            _logger.LogWarning("Failed to update Coach with ID {CoachId}. Errors: {Errors}", command.CoachId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return updateResult.Errors;
        }

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("Coaches", ct);

        _logger.LogInformation("Successfully updated Coach with ID {CoachId}.", command.CoachId);

        return Result.Updated;
    }



    public class UpdateCoachImageCommandHandler(IAppDbContext context, ILogger<UpdateCoachImageCommandHandler> logger, HybridCache cache) : IRequestHandler<UpdateCoachImageCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(UpdateCoachImageCommand command, CancellationToken ct)
        {
            logger.LogTrace("Handling Update Coach Image command for Coach ID {CoachId}.", command.coachId);

            var coach = await context.Coaches.Include(m => m.Person).
                ThenInclude(p=>p.Image).
                FirstOrDefaultAsync(m => m.Id == command.coachId, ct);

            if (coach is null)
            {
                logger.LogWarning("coach with ID {coachId} not found for image update.", command.coachId);
                return ApplicationErrors.MemberNotFound;
            }

            var updateResult = coach.UpdateImage(command.imageUrl);

            if (updateResult.IsError)
            {
                logger.LogWarning("Failed to update image for coach ID {coachId}. Errors: {Errors}", command.coachId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return updateResult.Errors;
            }

            await context.SaveChangesAsync(ct);

            await cache.RemoveByTagAsync("Coaches", ct);

            logger.LogInformation("Successfully updated image for coach ID {coachId}.", command.coachId);

            return Result.Updated;
        }
    }
}