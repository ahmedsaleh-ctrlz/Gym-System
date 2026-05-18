using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Attendances.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Attendances.Queries.GetAttendances;

public sealed record GetAttendancesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    string? SortBy = null,
    string? SortDirection = "desc") : ICachedQuery<Result<PaginatedList<AttendanceResponse>>>
{
    public string cacheKey =>
        $"Attendances:Page={PageNumber}:Size={PageSize}:Search={SearchTerm ?? "all"}:DateFrom={DateFrom?.ToString() ?? "null"}:DateTo={DateTo?.ToString() ?? "null"}:Sort={SortBy ?? "checkin"}:Direction={SortDirection}";

    public string[] cacheTag => ["Attendance"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
