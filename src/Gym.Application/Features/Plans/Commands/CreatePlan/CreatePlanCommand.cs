using Gym.Application.Features.Plans.Dtos;
using Gym.Domain.Common.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Plans.Commands.CreatePlan;

public sealed record CreatePlanCommand(string title, string? description, decimal cost, int durationInDays) : IRequest<Result<Created>>;
