using Gym.Application.Features.Coaches.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Coaches.Commands.CreateCoach;

public sealed record CreateCoachCommand(string FirstName,
        string LastName,
        DateTime DateOfBirth,
        string PhoneNumber,
        string ImageUrl,
        DateTime HireDate,
        string Email,
        string Password) : IRequest<Result<CoachResponse>>;