using Gym.Domain.Subscriptions.Enums;

namespace Gym.Api.Contracts.Subscriptions;

public sealed record UpdateSubscriptionStatusRequest(SubscriptionStatus NewStatus);
