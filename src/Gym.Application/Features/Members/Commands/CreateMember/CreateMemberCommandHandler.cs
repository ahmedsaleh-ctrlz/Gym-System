using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Members.Mappers;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;
using Gym.Domain.Members;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;

namespace Gym.Application.Features.Members.Commands.CreateMember;

public class CreateMemberCommandHandler(IAppDbContext context,
    ILogger<CreateMemberCommandHandler> logger,
    HybridCache cache,
    IIdentityService identityService,
    IImageStorage imageStorage) : IRequestHandler<CreateMemberCommand, Result<MemberResponse>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<CreateMemberCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;
    private readonly IIdentityService _identityService = identityService;
    private readonly IImageStorage _imageStorage = imageStorage;
    public async Task<Result<MemberResponse>> Handle(CreateMemberCommand command, CancellationToken ct)
    {
        _logger.LogTrace("Creating Member for email: {Email}", command.Email);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        string? userId = null;
        int? personId = null;
        string DefaultImageUrl = "images/default-image.png";
        string? promotedImageUrl = null;
        string? imageUrl = string.IsNullOrEmpty(command.ImageUrl) ? DefaultImageUrl : command.ImageUrl;
        Member? member = null;
        try
        {
            var memberResult = Member.Create(
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                command.PhoneNumber,
                imageUrl,
                command.JoinDate,
                command.Notes);

            if (memberResult.IsError)
            {
                return memberResult.Errors;
            }

            member = memberResult.Value;

            await _context.Members.AddAsync(member, ct);
            await _context.SaveChangesAsync(ct);

            personId = member.Person.Id;

            var userResult = await _identityService.CreateUserAsync(
                command.Email,
                command.Password,
                Role.Member,
                personId.Value,
                ct);

            if (userResult.IsError)
            {
                await transaction.RollbackAsync(ct);
                return userResult.Errors;
            }

            userId = userResult.Value;

            promotedImageUrl = await _imageStorage.PromoteTemporaryAsync(imageUrl, ct);
            var wasPromoted = !string.Equals(promotedImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase);
            var imageUpdateResult = member.UpdateImage(promotedImageUrl);

            if (imageUpdateResult.IsError)
            {
                await transaction.RollbackAsync(ct);
                if (wasPromoted)
                {
                    await _imageStorage.DeleteAsync(promotedImageUrl, ct);
                }

                return imageUpdateResult.Errors;
            }

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            if (!string.IsNullOrWhiteSpace(promotedImageUrl) &&
                !string.Equals(promotedImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase))
            {
                await _imageStorage.DeleteAsync(promotedImageUrl, ct);
            }

            if (userId is not null)
            {
                await _identityService.DeleteUserAsync(personId!.Value, ct);
            }

            _logger.LogError(ex, "Error creating member for {Email}", command.Email);
            throw;
        }

        await _cache.RemoveByTagAsync("AdminDashboard", ct);
        await _cache.RemoveByTagAsync("Member", ct);
        _logger.LogInformation("Successfully created Member with ID: {MemberId} and associated User ID: {UserId}", member.Id, userId);
        return member.ToDto();
    }
}
