using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions;

using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(int MemberId, int PlanId, DateOnly StartDate) : IRequest<Result<Subscription>>;