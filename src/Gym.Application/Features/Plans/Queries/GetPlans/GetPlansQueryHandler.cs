using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Plans.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Plans;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Plans.Queries.GetPlans;

public sealed class GetPlansQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPlansQuery, Result<PaginatedList<PlanDetailsResponse>>>
{
    public async Task<Result<PaginatedList<PlanDetailsResponse>>> Handle(GetPlansQuery query, CancellationToken ct)
    {
        var plansQuery = context.Plans.AsNoTracking().AsQueryable();

        plansQuery = ApplyFilters(plansQuery, query.SearchTerm);
        plansQuery = ApplySorting(plansQuery, query.SortBy, query.SortDirection);

        var count = await plansQuery.CountAsync(ct);

        var items = await plansQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PlanDetailsResponse
            {
                PlanId = p.Id,
                Title = p.Title,
                Description = p.Description,
                Cost = p.Cost,
                DurationInDays = p.DurationInDays,
                IsActive = p.IsActive
            })
            .ToListAsync(ct);

        return new PaginatedList<PlanDetailsResponse>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = count,
        };
    }

    private static IQueryable<Plan> ApplyFilters(IQueryable<Plan> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var normalized = searchTerm.Trim().ToLower();

        return query.Where(p =>
            p.Title.ToLower().Contains(normalized) ||
            (p.Description != null && p.Description.ToLower().Contains(normalized)));
    }

    private static IQueryable<Plan> ApplySorting(
        IQueryable<Plan> query,
        string? sortBy,
        string? sortDirection)
    {
        var isDesc = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "title" => isDesc ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "cost" => isDesc ? query.OrderByDescending(p => p.Cost) : query.OrderBy(p => p.Cost),
            "duration" => isDesc ? query.OrderByDescending(p => p.DurationInDays) : query.OrderBy(p => p.DurationInDays),
            _ => query.OrderBy(p => p.Id)
        };
    }
}
