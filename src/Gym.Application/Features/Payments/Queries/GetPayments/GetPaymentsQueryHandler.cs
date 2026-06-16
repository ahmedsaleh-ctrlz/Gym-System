using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Payments.Queries.GetPayments;

public sealed class GetPaymentsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPaymentsQuery, Result<PaginatedList<PaymentResponse>>>
{
    public async Task<Result<PaginatedList<PaymentResponse>>> Handle(GetPaymentsQuery query, CancellationToken ct)
    {
        var paymentsQuery = context.Payments
            .AsNoTracking()
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Member)
                    .ThenInclude(m => m.Person)
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .AsQueryable();

        paymentsQuery = ApplyFilters(paymentsQuery, query);
        paymentsQuery = ApplySorting(paymentsQuery, query.SortBy, query.SortDirection);

        var count = await paymentsQuery.CountAsync(ct);

        var items = await paymentsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PaymentResponse
            {
                PaymentId = p.Id,
                SubscriptionId = p.SubscriptionId,
                MemberId = p.Subscription.MemberId,
                MemberName = p.Subscription.Member.Person.FirstName + " " + p.Subscription.Member.Person.LastName,
                PlanName = p.Subscription.Plan!.Title,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.HasValue ? p.PaymentMethod.Value.ToString() : string.Empty,
                Status = p.Status.ToString(),
                PaidAtUtc = p.PaidAtUtc.HasValue ? DateTime.SpecifyKind(p.PaidAtUtc.Value, DateTimeKind.Utc) : null
            })
            .ToListAsync(ct);

        return new PaginatedList<PaymentResponse>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = count
        };
    }

    private static IQueryable<Payment> ApplyFilters(IQueryable<Payment> query, GetPaymentsQuery filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var normalized = filters.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Subscription.Member.Person.FirstName.ToLower().Contains(normalized) ||
                p.Subscription.Member.Person.LastName.ToLower().Contains(normalized) ||
                p.Subscription.Plan!.Title.ToLower().Contains(normalized));
        }

        if (filters.MemberId is not null)
        {
            query = query.Where(p => p.Subscription.MemberId == filters.MemberId);
        }

        if (filters.SubscriptionId is not null)
        {
            query = query.Where(p => p.SubscriptionId == filters.SubscriptionId);
        }

        if (filters.Status is not null)
        {
            query = query.Where(p => p.Status == filters.Status);
        }

        return query;
    }

    private static IQueryable<Payment> ApplySorting(IQueryable<Payment> query, string? sortBy, string? sortDirection)
    {
        var isDesc = sortDirection?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "membername" => isDesc
                ? query.OrderByDescending(p => p.Subscription.Member.Person.FirstName).ThenByDescending(p => p.Subscription.Member.Person.LastName)
                : query.OrderBy(p => p.Subscription.Member.Person.FirstName).ThenBy(p => p.Subscription.Member.Person.LastName),

            "amount" => isDesc
                ? query.OrderByDescending(p => p.Amount)
                : query.OrderBy(p => p.Amount),

            "status" => isDesc
                ? query.OrderByDescending(p => p.Status)
                : query.OrderBy(p => p.Status),

            _ => isDesc
                ? query.OrderByDescending(p => p.PaidAtUtc)
                : query.OrderBy(p => p.PaidAtUtc)
        };
    }
}