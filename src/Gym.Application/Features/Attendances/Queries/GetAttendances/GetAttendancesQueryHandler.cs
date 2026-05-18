using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Attendances.Dtos;
using Gym.Domain.Attendance;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Attendances.Queries.GetAttendances;

public sealed class GetAttendancesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAttendancesQuery, Result<PaginatedList<AttendanceResponse>>>
{
    public async Task<Result<PaginatedList<AttendanceResponse>>> Handle(GetAttendancesQuery query, CancellationToken ct)
    {
        var attendancesQuery = context.Attendances
            .AsNoTracking()
            .Include(a => a.Member)
                .ThenInclude(m => m.Person)
            .AsQueryable();

        attendancesQuery = ApplyFilters(attendancesQuery, query);
        attendancesQuery = ApplySorting(attendancesQuery, query.SortBy, query.SortDirection);

        var count = await attendancesQuery.CountAsync(ct);

        var items = await attendancesQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
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
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = count
        };
    }

    private static IQueryable<Attendance> ApplyFilters(IQueryable<Attendance> query, GetAttendancesQuery filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var normalized = filters.SearchTerm.Trim().ToLower();
            query = query.Where(a =>
                a.Member.Person.FirstName.ToLower().Contains(normalized) ||
                a.Member.Person.LastName.ToLower().Contains(normalized) ||
                a.Member.Person.PhoneNumber.ToLower().Contains(normalized));
        }

        if (filters.DateFrom is not null)
        {
            var fromDateTime = filters.DateFrom.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(a => a.CheckInAtUtc >= fromDateTime);
        }

        if (filters.DateTo is not null)
        {
            var toDateTime = filters.DateTo.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(a => a.CheckInAtUtc <= toDateTime);
        }

        return query;
    }

    private static IQueryable<Attendance> ApplySorting(IQueryable<Attendance> query, string? sortBy, string? sortDirection)
    {
        var isDesc = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "membername" => isDesc
                ? query.OrderByDescending(a => a.Member.Person.FirstName).ThenByDescending(a => a.Member.Person.LastName)
                : query.OrderBy(a => a.Member.Person.FirstName).ThenBy(a => a.Member.Person.LastName),

            _ => isDesc
                ? query.OrderByDescending(a => a.CheckInAtUtc)
                : query.OrderBy(a => a.CheckInAtUtc)
        };
    }
}
