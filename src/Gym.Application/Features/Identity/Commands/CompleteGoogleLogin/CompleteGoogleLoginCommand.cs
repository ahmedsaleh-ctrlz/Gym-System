using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Identity.Commands.CompleteGoogleLogin
{
    public sealed record CompleteGoogleLoginCommand(
    string ProviderKey,
    string? Email,
    bool EmailVerified
    ) : IRequest<Result<GoogleLoginOutcome>>;
}
