using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Application.Features.Subscriptions.Mappers;
using Gym.Domain.Common.Result;
using Gym.Domain.Members;
using Gym.Domain.Subscriptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Gym.Application.Features.Subscriptions.Queries.GetSubscriptions;

public class GetSubscriptionsQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetSubscriptionsQuery, Result<PaginatedList<SubscriptionResponse>>>
{
    public async Task<Result<PaginatedList<SubscriptionResponse>>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Subscriptions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(s => s.Member)
                .ThenInclude(m => m.Person)
            .Include(s => s.Plan)
            .AsQueryable();

        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => s.ToDto())
            .ToListAsync(cancellationToken);

        return new PaginatedList<SubscriptionResponse>
        {
            Items = items,
            TotalCount = count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }


    private static IQueryable<Subscription> ApplyFilters(
        IQueryable<Subscription> query,
        GetSubscriptionsQuery filters)
    {
      
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var normalized = filters.SearchTerm
                .Trim()
                .ToLower();

            query = query.Where(s =>
                s.Member.Person.FirstName.ToLower().Contains(normalized)
                || s.Plan!.Title.ToLower().Contains(normalized));
        }

      
        if (filters.Status is not null)
        {
            query = query.Where(s =>
                s.Status == filters.Status);
        }

       
        if (!string.IsNullOrWhiteSpace(filters.PlanName))
        {
            var normalizedPlan = filters.PlanName
                .Trim()
                .ToLower();

            query = query.Where(s =>
                s.Plan!.Title.ToLower().Contains(normalizedPlan));
        }

       
        if (filters.StartDateFrom is not null)
        {
            query = query.Where(s =>
                s.StartDate >= filters.StartDateFrom);
        }

     
        if (filters.StartDateTo is not null)
        {
            query = query.Where(s =>
                s.StartDate <= filters.StartDateTo);
        }

    
        if (filters.EndDateFrom is not null)
        {
            query = query.Where(s =>
                s.EndDate >= filters.EndDateFrom);
        }

        // End Date To
        if (filters.EndDateTo is not null)
        {
            query = query.Where(s =>
                s.EndDate <= filters.EndDateTo);
        }

        return query;
    }

    private static IQueryable<Subscription> ApplySorting(
        IQueryable<Subscription> query,
        GetSubscriptionsQuery filters)
    {
        var sortBy = filters.SortBy?
            .Trim()
            .ToLower();

        var isDescending =
            filters.SortDirection?.ToLower() == "desc";

        return sortBy switch
        {
            "membername" => isDescending
                ? query.OrderByDescending(s => s.Member.Person.FirstName)
                : query.OrderBy(s => s.Member.Person.FirstName),

            "planname" => isDescending
                ? query.OrderByDescending(s => s.Plan!.Title)
                : query.OrderBy(s => s.Plan!.Title),

            "startdate" => isDescending
                ? query.OrderByDescending(s => s.StartDate)
                : query.OrderBy(s => s.StartDate),

            "enddate" => isDescending
                ? query.OrderByDescending(s => s.EndDate)
                : query.OrderBy(s => s.EndDate),

            "status" => isDescending
                ? query.OrderByDescending(s => s.Status)
                : query.OrderBy(s => s.Status),

            _ => isDescending
                ? query.OrderByDescending(s => s.Id)
                : query.OrderBy(s => s.Id)
        };
    }


}
