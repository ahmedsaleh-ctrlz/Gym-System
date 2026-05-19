using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Payments.Queries.GetPaymentById;

public sealed record GetPaymentByIdQuery(int Id) : ICachedQuery<Result<PaymentResponse>>
{
    public string cacheKey => $"Payment_{Id}";

    public string[] cacheTag => ["Payments"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
