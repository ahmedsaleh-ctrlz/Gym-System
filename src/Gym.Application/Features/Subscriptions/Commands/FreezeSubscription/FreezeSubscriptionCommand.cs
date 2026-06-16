using System.Numerics;

using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.FreezeSubscription;

public sealed record FreezeSubscriptionCommand(int SubscriptionId, int FreezeDays) : IRequest<Result<Updated>>;