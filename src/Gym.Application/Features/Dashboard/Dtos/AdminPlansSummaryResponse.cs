namespace Gym.Application.Features.Dashboard.Dtos;

public sealed record AdminPlansSummaryResponse(
    string MostPopularPlan,
    int MostPopularPlanSubscriptionsCount,
    string HighestRevenuePlan,
    decimal HighestPlanRevenue,
    IReadOnlyList<AdminPlanPerformance> Plans);

public sealed record AdminPlanPerformance(
    int PlanId,
    string PlanName,
    int SubscriptionsCount,
    decimal Revenue);