namespace Gym.Application.Features.Dashboard.Dtos;
public sealed record AdminSubscriptionsSummaryResponse(
    int ActiveSubscriptions,
    int FrozenSubscriptions,
    int ScheduledSubscriptions,
    int ExpiredSubscriptions,

    int NewSubscriptionsThisMonth,
    int NewSubscriptionsPreviousMonth,

    int ExpiringSubscriptionsNext7Days,
    int ExpiringSubscriptionsNext30Days,

    int ExpiredSubscriptionsThisMonth
);
