using Gym.Domain.Common.Result;
using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.RenewSubscription;

public sealed record RenewSubscriptionCommand(int memberId, int planId) : IRequest<Result<Created>>;
