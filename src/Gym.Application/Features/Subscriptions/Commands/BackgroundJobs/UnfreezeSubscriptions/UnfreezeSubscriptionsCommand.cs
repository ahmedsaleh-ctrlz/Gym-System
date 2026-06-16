using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.UnfreezeSubscriptions;

public sealed record UnfreezeSubscriptionsCommand : IRequest;