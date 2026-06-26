namespace Gym.Application.Features.Payments.Dtos;

public sealed record StripePaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret);