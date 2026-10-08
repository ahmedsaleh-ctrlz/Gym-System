namespace Gym.Application.Features.Dashboard.Dtos;
public sealed record AdminRecentCheckInsResponse(
    IReadOnlyList<RecentCheckIn> CheckIns);