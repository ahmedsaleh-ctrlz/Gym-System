using Gym.Application.Features.Dashboard.Dtos;

namespace Gym.Application.Features.Dashboard.Dtos;

public sealed record AdminRevenueSummaryResponse(
    decimal CurrentPeriodRevenue,
    decimal PreviousPeriodRevenue,
    decimal GrowthPercentage,
    IReadOnlyList<RevenuePoint> Data);