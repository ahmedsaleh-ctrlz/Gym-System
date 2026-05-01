using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Members.Commands.UpdateMember;

public sealed class UpdateMemberCommandHandler(IAppDbContext context,
                                               ILogger<UpdateMemberCommandHandler> logger,
                                               HybridCache cache) : IRequestHandler<UpdateMemberCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UpdateMemberCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateMemberCommand command, CancellationToken ct)
    {
        _logger.LogTrace("Handling Update Member command for Member ID {MemberId}.", command.MemberId);

        var memberResult = await _context.Members.Include(m => m.Person).FirstOrDefaultAsync(m => m.Id == command.MemberId, ct);

        if (memberResult is null)
        {
            _logger.LogWarning("Member with ID {MemberId} not found for update.", command.MemberId);
            return ApplicationErrors.MemberNotFound;
        }

        var updateResult = memberResult.UpdateInfo(
            command.FirstName, command.LastName, command.DateOfBirth, command.PhoneNumber, command.JoinDate,
            command.Notes);

        if (updateResult.IsError)
        {
            _logger.LogWarning("Failed to update Member with ID {MemberId}. Errors: {Errors}", command.MemberId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return updateResult.Errors;
        }

        await _cache.RemoveByTagAsync("Member", ct);

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Successfully updated Member with ID {MemberId}.", command.MemberId);

        return Result.Updated;
    }



    public class UpdateMemberImageCommandHandler(IAppDbContext context, ILogger<UpdateMemberImageCommandHandler> logger, HybridCache cache) : IRequestHandler<UpdateMemberImageCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(UpdateMemberImageCommand command, CancellationToken ct)
        {
            logger.LogTrace("Handling Update Member Image command for Member ID {MemberId}.", command.memberId);

            var memberResult = await context.Members.Include(m => m.Person).
                FirstOrDefaultAsync(m => m.Id == command.memberId, ct);

            if (memberResult is null)
            {
                logger.LogWarning("Member with ID {MemberId} not found for image update.", command.memberId);
                return ApplicationErrors.MemberNotFound;
            }

            var updateResult = memberResult.UpdateImage(command.imageUrl);

            if (updateResult.IsError)
            {
                logger.LogWarning("Failed to update image for Member ID {MemberId}. Errors: {Errors}", command.memberId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return updateResult.Errors;
            }

            await cache.RemoveByTagAsync("Member", ct);

            await context.SaveChangesAsync(ct);

            logger.LogInformation("Successfully updated image for Member ID {MemberId}.", command.memberId);

            return Result.Updated;
        }
    }
}