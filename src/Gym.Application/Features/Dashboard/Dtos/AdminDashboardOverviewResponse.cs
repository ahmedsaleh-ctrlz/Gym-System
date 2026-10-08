namespace Gym.Application.Features.Dashboard.Dtos;

public sealed record AdminDashboardOverviewResponse(
    int TotalMembers,

    int ActiveSubscriptions,
    int ExpiringSubscriptionsNext7Days,

    decimal ThisMonthRevenue,
    decimal PreviousMonthRevenue,
    decimal RevenueGrowthPercentage,

    int TodayAttendanceCount,

    int PendingPaymentsCount,
    int UnderReviewPaymentsCount,

    string MostPopularPlan,
    int MostPopularPlanSubscriptionsCount
);