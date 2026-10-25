using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Identity.Commands.RegisterGoogleMember;

public sealed record RegisterGoogleMemberCommand(
    string ProviderKey,
    string Email,
    bool EmailVerified,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    string? ImageUrl
) : IRequest<Result<TokenResponse>>;