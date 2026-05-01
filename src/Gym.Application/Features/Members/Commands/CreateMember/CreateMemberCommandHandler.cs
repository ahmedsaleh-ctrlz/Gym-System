using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Members.Mappers;
using Gym.Domain.Common.Constants.Enums;
using Gym.Domain.Common.Result;
using Gym.Domain.Members;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Members.Commands.CreateMember;

public class CreateMemberCommandHandler(IAppDbContext context,
    ILogger<CreateMemberCommandHandler> logger,
    HybridCache cache,
    IIdentityService identityService) : IRequestHandler<CreateMemberCommand, Result<MemberResponse>>

{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<CreateMemberCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;
    private readonly IIdentityService _identityService = identityService;

    public async Task<Result<MemberResponse>> Handle(CreateMemberCommand command, CancellationToken ct)
    {
        _logger.LogTrace("Creating Member for email: {Email}", command.email);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        int? userId = null;

        try
        {
            var userResult = await _identityService.CreateUserAsync(
            command.email, command.password, Role.Member, ct);

            if (userResult.IsError)
                return userResult.Errors;

            userId = userResult.Value;

            var memberResult = Member.Create(
            command.firstName,
            command.lastName,
            command.dateOfBirth,
            command.phoneNumber,
            command.imageUrl,
            command.joinDate,
            command.notes,
            userId.Value);

            if (memberResult.IsError)
                return memberResult.Errors;

            var member = memberResult.Value;

            await _context.Members.AddAsync(member, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await _cache.RemoveByTagAsync("Member", ct);

            _logger.LogInformation("Created member {MemberId} linked to user {UserId}",
            member.Id, userId);

            return member.ToDto();

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            
            if (userId is not null)
            {
                _logger.LogWarning("Compensating: deleting identity user {UserId} after DB failure", userId);
                await _identityService.DeleteUserAsync(userId.Value, ct);
            }

            _logger.LogError(ex, "Error creating member for {Email}", command.email);
            throw;
        }

    }
}