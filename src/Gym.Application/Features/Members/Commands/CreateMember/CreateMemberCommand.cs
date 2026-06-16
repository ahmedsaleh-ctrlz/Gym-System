using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Members.Commands.CreateMember;

public sealed record CreateMemberCommand(string FirstName,
        string LastName,
        DateTime DateOfBirth,
        string PhoneNumber,
        string ImageUrl,
        DateTime JoinDate,
        string? Notes,
        string Email,
        string Password) : IRequest<Result<MemberResponse>>;