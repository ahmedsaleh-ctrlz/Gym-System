using Gym.Application.Features.Payments.Dtos;

namespace Gym.Application.Common.Interfaces
{
    public interface IStripePaymentService
    {
        Task<StripePaymentIntentResult> CreatePaymentIntentAsync(decimal amount, CancellationToken cancellationToken = default);
    }
}