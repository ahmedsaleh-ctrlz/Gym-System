using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Members;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Members.Queries.GetMembers;

public class GetMembersQueryHandler(IAppDbContext context) : IRequestHandler<GetMembersQuery, Result<PaginatedList<MemberResponse>>>
{
    public async Task<Result<PaginatedList<MemberResponse>>> Handle(GetMembersQuery query, CancellationToken ct)
    {
        var membersQuery = context.Members.AsNoTracking().AsQueryable();
        var memberQuery = ApplyFilters(membersQuery, query.SearchTerm);

        memberQuery = ApplySorting(memberQuery, query.SortBy, query.SortDirection);

        var count = await memberQuery.CountAsync(ct);

        var items = await memberQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new MemberResponse
            {
                MemberId = m.Id,
                FirstName = m.Person.FirstName,
                LastName = m.Person.LastName,
                DateOfBirth = m.Person.DateOfBirth,
                PhoneNumber = m.Person.PhoneNumber,
                ImageUrl = m.Person.Image.ImageUrl,
                JoinDate = m.JoinDate,
                Notes = m.Notes
            })
            .ToListAsync(ct);

        return new PaginatedList<MemberResponse>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = count,
        };
    }

    private static IQueryable<Member> ApplyFilters(IQueryable<Member> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var normalized = searchTerm.Trim().ToLower();

        return query.Where(m =>
            m.Person.FirstName.ToLower().Contains(normalized) ||
            m.Person.LastName.ToLower().Contains(normalized) ||
            m.Person.PhoneNumber.ToLower().Contains(normalized));
    }

    private static IQueryable<Member> ApplySorting(
    IQueryable<Member> query,
    string? sortBy,
    string? sortDirection)
    {
        var isDesc = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "name" => isDesc
                ? query.OrderByDescending(m => m.Person.FirstName).ThenByDescending(m => m.Person.LastName)
                : query.OrderBy(m => m.Person.FirstName).ThenBy(m => m.Person.LastName),

            "joindate" => isDesc
                ? query.OrderByDescending(m => m.JoinDate)
                : query.OrderBy(m => m.JoinDate),

            _ => query.OrderBy(m => m.Id)
        };
    }
}