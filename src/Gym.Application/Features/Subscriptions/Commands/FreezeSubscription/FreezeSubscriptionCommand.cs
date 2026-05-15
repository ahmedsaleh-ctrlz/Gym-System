using Gym.Domain.Common.Result;
using MediatR;
using System.Numerics;


namespace Gym.Application.Features.Subscriptions.Commands.FreezeSubscription;
public sealed record FreezeSubscriptionCommand(int subscriptionId,int FreezeDays) : IRequest<Result<Updated>>;

