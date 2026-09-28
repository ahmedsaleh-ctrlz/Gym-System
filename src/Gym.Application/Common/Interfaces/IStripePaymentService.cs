using Gym.Application.Features.Payments.Dtos;

namespace Gym.Application.Common.Interfaces
{
    public interface IStripePaymentService
    {
        Task<StripePaymentIntentResult> CreatePaymentIntentAsync(decimal amount, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
        Task<StripePaymentIntentResult> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
        Task<StripePaymentIntentResult> UpdatePaymentIntentAsync(
        string paymentIntentId,
        decimal amount,
        Dictionary<string, string>? metadata,
        CancellationToken ct);
        }
}