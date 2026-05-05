using Gym.Application.Features.Coaches.Dtos;
using Gym.Domain.Common.Result;
using MediatR;

namespace Gym.Application.Features.Coaches.Commands.CreateCoach;

public sealed record CreateCoachCommand(string firstName,
        string lastName,
        DateTime dateOfBirth,
        string phoneNumber,
        string imageUrl,
        DateTime HireDate,
        string email,
        string password) : IRequest<Result<CoachResponse>>;
