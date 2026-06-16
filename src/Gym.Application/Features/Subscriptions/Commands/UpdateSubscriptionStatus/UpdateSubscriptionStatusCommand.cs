using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.UpdateSubscriptionStatus;

public sealed record UpdateSubscriptionStatusCommand(int SubscriptionId, SubscriptionStatus NewStatus) : IRequest<Result<Updated>>;