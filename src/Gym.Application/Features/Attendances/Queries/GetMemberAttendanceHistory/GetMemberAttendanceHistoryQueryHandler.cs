using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Attendances.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Attendances.Queries.GetMemberAttendanceHistory;

public sealed class GetMemberAttendanceHistoryQueryHandler(IAppDbContext context)
    : IRequestHandler<GetMemberAttendanceHistoryQuery, Result<PaginatedList<AttendanceResponse>>>
{
    public async Task<Result<PaginatedList<AttendanceResponse>>> Handle(GetMemberAttendanceHistoryQuery query, CancellationToken ct)
    {
        var memberExists = await context.Members.AnyAsync(m => m.Id == query.MemberId, ct);
        if (!memberExists)
        {
            return ApplicationErrors.MemberNotFound;
        }

        var attendanceQuery = context.Attendances
            .AsNoTracking()
            .Include(a => a.Member)
                .ThenInclude(m => m.Person)
            .Where(a => a.MemberId == query.MemberId);

        if (query.DateFrom is not null)
        {
            var fromDateTime = query.DateFrom.Value.ToDateTime(TimeOnly.MinValue);
            attendanceQuery = attendanceQuery.Where(a => a.CheckInAtUtc >= fromDateTime);
        }

        if (query.DateTo is not null)
        {
            var toDateTime = query.DateTo.Value.ToDateTime(TimeOnly.MaxValue);
            attendanceQuery = attendanceQuery.Where(a => a.CheckInAtUtc <= toDateTime);
        }

        attendanceQuery = query.SortDirection?.ToLower() == "asc"
            ? attendanceQuery.OrderBy(a => a.CheckInAtUtc)
            : attendanceQuery.OrderByDescending(a => a.CheckInAtUtc);

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var count = await attendanceQuery.CountAsync(ct);

        var items = await attendanceQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AttendanceResponse
            {
                AttendanceId = a.Id,
                MemberId = a.MemberId,
                MemberName = a.Member.Person.FirstName + " " + a.Member.Person.LastName,
                CheckInAtUtc = a.CheckInAtUtc
            })
            .ToListAsync(ct);

        return new PaginatedList<AttendanceResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = count
        };
    }
}