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

        await _context.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("AdminDashboard:Overview", ct);
        await _cache.RemoveByTagAsync("AdminDashboard:MembersSummary", ct);
        await _cache.RemoveByTagAsync("Member", ct);

        _logger.LogInformation("Successfully updated Member with ID {MemberId}.", command.MemberId);

        return Result.Updated;
    }

    public class UpdateMemberImageCommandHandler(
        IAppDbContext context,
        ILogger<UpdateMemberImageCommandHandler> logger,
        HybridCache cache,
        IImageStorage imageStorage) : IRequestHandler<UpdateMemberImageCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(UpdateMemberImageCommand command, CancellationToken ct)
        {
            logger.LogTrace("Handling Update Member Image command for Member ID {MemberId}.", command.MemberId);

            var memberResult = await context.Members
                .Include(m => m.Person)
                .ThenInclude(p => p.Image).
                FirstOrDefaultAsync(m => m.Id == command.MemberId, ct);

            if (memberResult is null)
            {
                logger.LogWarning("Member with ID {MemberId} not found for image update.", command.MemberId);
                return ApplicationErrors.MemberNotFound;
            }

            var oldImageUrl = memberResult.Person.Image.ImageUrl;
            var promotedImageUrl = await imageStorage.PromoteTemporaryAsync(command.ImageUrl, ct);
            var updateResult = memberResult.UpdateImage(promotedImageUrl);

            if (updateResult.IsError)
            {
                if (!string.Equals(promotedImageUrl, command.ImageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    await imageStorage.DeleteAsync(promotedImageUrl, ct);
                }

                logger.LogWarning("Failed to update image for Member ID {MemberId}. Errors: {Errors}", command.MemberId, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return updateResult.Errors;
            }

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch
            {
                if (!string.Equals(promotedImageUrl, command.ImageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    await imageStorage.DeleteAsync(promotedImageUrl, ct);
                }

                throw;
            }

            if (!string.Equals(oldImageUrl, promotedImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                await imageStorage.DeleteAsync(oldImageUrl, ct);
            }

            await cache.RemoveByTagAsync("Member", ct);
            await cache.RemoveByTagAsync("AdminDashboard:Overview", ct);
            await cache.RemoveByTagAsync("AdminDashboard:MembersSummary", ct);

            logger.LogInformation("Successfully updated image for Member ID {MemberId}.", command.MemberId);

            return Result.Updated;
        }
    }
}
