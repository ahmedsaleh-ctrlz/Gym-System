using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Members.Mappers;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;
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
        _logger.LogTrace("Creating Member for email: {Email}", command.Email);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        string? userId = null;
        int? personId = null;

        try
        {
            var memberResult = Member.Create(
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                command.PhoneNumber,
                command.ImageUrl,
                command.JoinDate,
                command.Notes);

            if (memberResult.IsError)
            {
                return memberResult.Errors;
            }

            var member = memberResult.Value;

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
                _logger.LogError("Failed to create user for Member with email: {Email}. Errors: {Errors}", command.Email, userResult.Errors);
                await transaction.RollbackAsync(ct);
                return userResult.Errors;
            }

            userId = userResult.Value;

            await transaction.CommitAsync(ct);
            await _cache.RemoveByTagAsync("AdminDashboard", ct);
            await _cache.RemoveByTagAsync("Member", ct);

            _logger.LogInformation("Successfully created Member with ID: {MemberId} and associated User ID: {UserId}", member.Id, userId);

            return member.ToDto();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            if (userId is not null)
            {
                await _identityService.DeleteUserAsync(personId!.Value, ct);
            }

            _logger.LogError(ex, "Error creating member for {Email}", command.Email);
            throw;
        }
    }
}