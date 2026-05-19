using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;

namespace Gym.Application.Features.Payments.Queries.GetPayments;

public sealed record GetPaymentsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    int? MemberId = null,
    int? SubscriptionId = null,
    PaymentStatus? Status = null,
    string? SortBy = null,
    string? SortDirection = "desc") : ICachedQuery<Result<PaginatedList<PaymentResponse>>>
{
    public string cacheKey =>
        $"Payments:Page={PageNumber}:Size={PageSize}:Search={SearchTerm ?? "all"}:Member={MemberId?.ToString() ?? "all"}:Subscription={SubscriptionId?.ToString() ?? "all"}:Status={Status?.ToString() ?? "all"}:Sort={SortBy ?? "paidat"}:Direction={SortDirection}";

    public string[] cacheTag => ["Payments"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
