namespace Gym.Api.Contracts.Subscriptions;

public sealed record CreateSubscriptionRequest(
    int MemberId,
    int PlanId,
    DateOnly StartDate);
