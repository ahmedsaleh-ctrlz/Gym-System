using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Attendances.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Attendances.Queries.GetMemberAttendanceHistory;

public sealed record GetMemberAttendanceHistoryQuery(
    int MemberId,
    int PageNumber = 1,
    int PageSize = 10,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    string? SortDirection = "desc") : ICachedQuery<Result<PaginatedList<AttendanceResponse>>>
{
    public string CacheKey =>
        $"AttendanceHistory:Member={MemberId}:Page={PageNumber}:Size={PageSize}:DateFrom={DateFrom?.ToString() ?? "null"}:DateTo={DateTo?.ToString() ?? "null"}:Direction={SortDirection}";

    public string[] CacheTag => ["Attendance"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}