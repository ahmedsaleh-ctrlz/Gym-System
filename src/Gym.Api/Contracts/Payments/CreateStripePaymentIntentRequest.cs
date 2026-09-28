namespace Gym.Api.Contracts.Payments
{
    public sealed record CreateStripePaymentIntentRequest(int PaymentId, int? PromoCodeId = null);
}
