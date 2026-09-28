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
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = "egp",
            AutomaticPaymentMethods = new()
            {
                Enabled = true
            },
            Metadata = metadata
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return new StripePaymentIntentResult(paymentIntent.Id, paymentIntent.ClientSecret);
    }

    public async Task<StripePaymentIntentResult> GetPaymentIntentAsync(
    string paymentIntentId,
    CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;

        var service = new PaymentIntentService();

        var paymentIntent = await service.GetAsync(
            paymentIntentId,
            cancellationToken: cancellationToken);

        return new StripePaymentIntentResult(
            paymentIntent.Id,
            paymentIntent.ClientSecret);
    }

    public async Task<StripePaymentIntentResult> UpdatePaymentIntentAsync(
    string paymentIntentId,
    decimal amount,
    Dictionary<string, string>? metadata,
    CancellationToken ct)
    {
        var service = new PaymentIntentService();

        var options = new PaymentIntentUpdateOptions
        {
            Amount = (long)(amount * 100),
            Metadata = metadata
        };

        var paymentIntent = await service.UpdateAsync(
            paymentIntentId,
            options,
            cancellationToken: ct);

        return new StripePaymentIntentResult(
            paymentIntent.Id,
            paymentIntent.ClientSecret);
    }
}