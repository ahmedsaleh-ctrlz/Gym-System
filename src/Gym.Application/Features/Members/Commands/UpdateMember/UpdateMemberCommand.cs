using FluentValidation;

using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Members.Commands.UpdateMember;

public sealed record UpdateMemberCommand(
    int MemberId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    DateTime JoinDate,
    string? Notes) : IRequest<Result<Updated>>;

public sealed record UpdateMemberImageCommand(int MemberId, string ImageUrl) : IRequest<Result<Updated>>;