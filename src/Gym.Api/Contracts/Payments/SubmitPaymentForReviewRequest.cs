using Gym.Domain.Payments.Enums;

namespace Gym.Api.Contracts.Payments;

public sealed record SubmitPaymentForReviewRequest(
    int PaymentId,
    PaymentMethod PaymentMethod,
    string PaymentReference,
    int? PromoCodeId);