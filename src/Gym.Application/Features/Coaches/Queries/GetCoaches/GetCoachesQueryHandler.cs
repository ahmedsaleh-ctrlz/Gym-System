using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Coaches.Dtos;
using Gym.Domain.Coaches;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Coaches.Queries.GetCoaches;

public class GetCoachesQueryHandler(IAppDbContext context) : IRequestHandler<GetCoachesQuery, Result<PaginatedList<CoachResponse>>>
{
    public async Task<Result<PaginatedList<CoachResponse>>> Handle(GetCoachesQuery query, CancellationToken ct)
    {
        var coachQuery = context.Coaches.AsNoTracking().AsQueryable();

        coachQuery = ApplyFilters(coachQuery, query.SearchTerm);
        coachQuery = ApplySorting(coachQuery, query.SortBy, query.SortDirection);

        var count = await coachQuery.CountAsync(ct);

        var items = await coachQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new CoachResponse
            {
                CoachId = m.Id,
                FirstName = m.Person.FirstName,
                LastName = m.Person.LastName,
                DateOfBirth = m.Person.DateOfBirth,
                PhoneNumber = m.Person.PhoneNumber,
                ImageUrl = m.Person.Image.ImageUrl,
                HireDate = m.HireDate
            })
            .ToListAsync(ct);

        return new PaginatedList<CoachResponse>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = count,
        };
    }

    private static IQueryable<Coach> ApplyFilters(IQueryable<Coach> query, string? searchTerm)
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

    private static IQueryable<Coach> ApplySorting(
    IQueryable<Coach> query,
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
                ? query.OrderByDescending(m => m.HireDate)
                : query.OrderBy(m => m.HireDate),

            _ => query.OrderBy(m => m.Id)
        };
    }
}