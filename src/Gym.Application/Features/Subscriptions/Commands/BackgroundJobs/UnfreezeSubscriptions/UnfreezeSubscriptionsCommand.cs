using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.UnfreezeSubscriptions;

public sealed record UnfreezeSubscriptionsCommand : IRequest;
