using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions;
using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(int memberId, int planId, DateOnly startDate) : IRequest<Result<Subscription>>;

