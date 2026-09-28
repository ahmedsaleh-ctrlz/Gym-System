using System;
using System.Collections.Generic;
using System.Text;

using Gym.Domain.Common.Result;
using Gym.Domain.PromoCodes;

using MediatR;

namespace Gym.Application.Features.PromoCodes.Commands.ActivatePromoCode
{
    public sealed record ActivatePromoCodeCommand(
    int Id)
    : IRequest<Result<Updated>>;
}
