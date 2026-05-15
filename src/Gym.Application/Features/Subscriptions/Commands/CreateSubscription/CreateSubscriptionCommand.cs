using Gym.Domain.Common.Result;
using Gym.Domain.Plans;
using Gym.Domain.Subscriptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(int memberId, Plan plan, DateOnly startDate) : IRequest<Result<Subscription>>;

