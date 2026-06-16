using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.RenewSubscription;

public sealed record RenewSubscriptionCommand(int MemberId, int PlanId) : IRequest<Result<Created>>;