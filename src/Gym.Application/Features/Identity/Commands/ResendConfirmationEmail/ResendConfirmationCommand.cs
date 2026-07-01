using System;
using System.Collections.Generic;
using System.Text;

using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Identity.Commands.ResendConfirmationEmail;

public sealed record ResendConfirmationCommand(string Email) : IRequest<Result<Created>>;
