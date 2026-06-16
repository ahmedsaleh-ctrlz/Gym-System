using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Features.Plans.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Plans.Commands.CreatePlan;

public sealed record CreatePlanCommand(string Title, string? Description, decimal Cost, int DurationInDays, int AllowedFreezeCount, int MaxTotalFreezeDays) : IRequest<Result<Created>>;