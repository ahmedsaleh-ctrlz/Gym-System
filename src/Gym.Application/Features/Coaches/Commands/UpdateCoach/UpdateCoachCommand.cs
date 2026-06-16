using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Coaches.Commands.UpdateCoach;

public sealed record UpdateCoachCommand(
    int CoachId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    DateTime HireDate) : IRequest<Result<Updated>>;

public sealed record UpdateCoachImageCommand(int CoachId, string ImageUrl) : IRequest<Result<Updated>>;