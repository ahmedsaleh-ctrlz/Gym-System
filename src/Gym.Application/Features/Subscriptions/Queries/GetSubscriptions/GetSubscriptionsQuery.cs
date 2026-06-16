using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions.Enums;

namespace Gym.Application.Features.Subscriptions.Queries.GetSubscriptions;

public sealed record GetSubscriptionsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    SubscriptionStatus? Status = null,
    string? PlanName = null,
    DateOnly? StartDateFrom = null,
    DateOnly? StartDateTo = null,
    DateOnly? EndDateFrom = null,
    DateOnly? EndDateTo = null,
    string? SortBy = null,
    string? SortDirection = "Asce"
) : ICachedQuery<Result<PaginatedList<SubscriptionResponse>>>
{
    public string CacheKey =>
        $"Subscriptions:" +
        $"Page={PageNumber}:" +
        $"Size={PageSize}:" +
        $"Search={SearchTerm ?? "all"}:" +
        $"Status={Status?.ToString() ?? "all"}:" +
        $"Plan={PlanName ?? "all"}:" +
        $"StartFrom={StartDateFrom?.ToString() ?? "null"}:" +
        $"StartTo={StartDateTo?.ToString() ?? "null"}:" +
        $"EndFrom={EndDateFrom?.ToString() ?? "null"}:" +
        $"EndTo={EndDateTo?.ToString() ?? "null"}:" +
        $"Sort={SortBy ?? "Id"}:" +
        $"Direction={SortDirection}";

    public string[] CacheTag => ["Subscriptions"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}