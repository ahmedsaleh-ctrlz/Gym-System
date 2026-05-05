using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Coaches.Dtos;
using Gym.Application.Features.Coaches.Mappers;
using Gym.Domain.Common.Constants.Enums;
using Gym.Domain.Common.Result;
using Gym.Domain.Coachs;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Coaches.Commands.CreateCoach;

public class CreateCoachCommandHandler(IAppDbContext context,
    ILogger<CreateCoachCommandHandler> logger,
    HybridCache cache,
    IIdentityService identityService) : IRequestHandler<CreateCoachCommand, Result<CoachResponse>>

{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<CreateCoachCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;
    private readonly IIdentityService _identityService = identityService;

    public async Task<Result<CoachResponse>> Handle(CreateCoachCommand command, CancellationToken ct)
    {
        _logger.LogTrace("Creating Coach for email: {Email}", command.email);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        string? userId = null;
        int? personId = null;  

        try
        {
            
            var CoachResult = Coach.Create(
                command.firstName,
                command.lastName,
                command.dateOfBirth,
                command.phoneNumber,
                command.imageUrl,
                command.HireDate);

            if (CoachResult.IsError)
                return CoachResult.Errors;

            var coach = CoachResult.Value;

            
            await _context.Coaches.AddAsync(coach, ct);
            await _context.SaveChangesAsync(ct);

            personId = coach.Person.Id;

            
            var userResult = await _identityService.CreateUserAsync(
                command.email,
                command.password,
                Role.Coach,
                personId.Value,
                ct);

            if (userResult.IsError)
            {
                _logger.LogError("Failed to create user for Coach with email: {Email}. Errors: {Errors}", command.email, userResult.Errors);   
                await transaction.RollbackAsync(ct);
                return userResult.Errors;
            }

            userId = userResult.Value;

           
            await transaction.CommitAsync(ct);

            await _cache.RemoveByTagAsync("Coach", ct);

            _logger.LogInformation("Successfully created Coach with ID: {CoachId} and associated User ID: {UserId}", coach.Id, userId);

            return coach.ToDto();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            if (userId is not null)
            {
                await _identityService.DeleteUserAsync(personId!.Value, ct);
            }

            _logger.LogError(ex, "Error creating Coach for {Email}", command.email);
            throw;
        }
    }
}
