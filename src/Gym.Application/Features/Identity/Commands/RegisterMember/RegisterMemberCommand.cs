using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Identity.Commands.RegisterMember;

public sealed record RegisterMemberCommand(string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    string ImageUrl,
    string Email,
    string Password) : IRequest<Result<Created>>;
