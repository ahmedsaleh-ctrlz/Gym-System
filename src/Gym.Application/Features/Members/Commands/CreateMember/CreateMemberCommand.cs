using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;
using MediatR;

namespace Gym.Application.Features.Members.Commands.CreateMember;

public sealed record CreateMemberCommand(string firstName,
        string lastName,
        DateTime dateOfBirth,
        string phoneNumber,
        string imageUrl,
        DateTime joinDate,
        string? notes,
        string email,
        string password) : IRequest<Result<MemberResponse>>;
