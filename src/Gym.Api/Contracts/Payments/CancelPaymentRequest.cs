namespace Gym.Api.Contracts.Payments;

public sealed record CancelPaymentRequest(
    int PaymentId);