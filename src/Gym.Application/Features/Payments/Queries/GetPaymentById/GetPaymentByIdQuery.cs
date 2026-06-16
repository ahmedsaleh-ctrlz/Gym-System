using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Payments.Queries.GetPaymentById;

public sealed record GetPaymentByIdQuery(int Id) : ICachedQuery<Result<PaymentResponse>>
{
    public string CacheKey => $"Payment_{Id}";

    public string[] CacheTag => ["Payments"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}