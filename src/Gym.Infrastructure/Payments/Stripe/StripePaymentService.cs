using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Dtos;
using Gym.Infrastructure.Settings;
using Microsoft.Extensions.Options;

using Stripe;

namespace Gym.Infrastructure.Payments.Stripe;

public sealed class StripePaymentService(IOptions<StripeSettings> stripeOptions) : IStripePaymentService
{
    public async Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = "usd",
            AutomaticPaymentMethods = new()
            {
                Enabled = true
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return new StripePaymentIntentResult(paymentIntent.Id, paymentIntent.ClientSecret);
    }
}