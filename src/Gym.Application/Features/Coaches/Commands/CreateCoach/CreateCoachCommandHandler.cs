using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Coaches.Dtos;
using Gym.Application.Features.Coaches.Mappers;
using Gym.Domain.Coaches;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Coaches.Commands.CreateCoach;

public class CreateCoachCommandHandler(
    IAppDbContext context,
    ILogger<CreateCoachCommandHandler> logger,
    HybridCache cache,
    IIdentityService identityService,
    IImageStorage imageStorage) : IRequestHandler<CreateCoachCommand, Result<CoachResponse>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<CreateCoachCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;
    private readonly IIdentityService _identityService = identityService;
    private readonly IImageStorage _imageStorage = imageStorage;

    public async Task<Result<CoachResponse>> Handle(CreateCoachCommand command, CancellationToken ct)
    {
        _logger.LogTrace("Creating Coach for email: {Email}", command.Email);

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<Result<CoachResponse>>(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            string? userId = null;
            int? personId = null;
            string? promotedImageUrl = null;
            string? imageUrl = command.ImageUrl;

            try
            {
                var coachResult = Coach.Create(
                    command.FirstName,
                    command.LastName,
                    command.DateOfBirth,
                    command.PhoneNumber,
                    imageUrl,
                    command.HireDate);

                if (coachResult.IsError)
                {
                    return coachResult.Errors;
                }

                var coach = coachResult.Value;

                await _context.Coaches.AddAsync(coach, ct);
                await _context.SaveChangesAsync(ct);

                personId = coach.Person.Id;

                var userResult = await _identityService.CreateUserAsync(
                    command.Email,
                    command.Password,
                    Role.Coach,
                    personId.Value,
                    ct);

                if (userResult.IsError)
                {
                    _logger.LogError("Failed to create user for Coach with email: {Email}. Errors: {Errors}", command.Email, userResult.Errors);
                    await transaction.RollbackAsync(ct);
                    return userResult.Errors;
                }

                userId = userResult.Value;

                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    promotedImageUrl = await _imageStorage.PromoteTemporaryAsync(imageUrl, ct);
                    var wasPromoted = !string.Equals(promotedImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase);
                    var imageUpdateResult = coach.UpdateImage(promotedImageUrl);

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
                }

                await transaction.CommitAsync(ct);

                await _cache.RemoveByTagAsync("Coach", ct);

                _logger.LogInformation("Successfully created Coach with ID: {CoachId} and associated User ID: {UserId}", coach.Id, userId);

                return coach.ToDto();
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

                _logger.LogError(ex, "Error creating Coach for {Email}", command.Email);
                throw;
            }
        });
    }
}