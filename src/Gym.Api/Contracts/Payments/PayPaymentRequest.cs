using Gym.Domain.Payments.Enums;

namespace Gym.Api.Contracts.Payments;

public sealed record PayPaymentRequest(
    int PaymentId,
    PaymentMethod PaymentMethod);