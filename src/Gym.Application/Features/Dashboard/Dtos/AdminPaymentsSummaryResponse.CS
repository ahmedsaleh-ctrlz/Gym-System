namespace Gym.Application.Features.Dashboard.Dtos;
public sealed record AdminPaymentsSummaryResponse(
    int PendingPaymentsCount,
    int UnderReviewPaymentsCount,
    int PaidPaymentsCount,
    int FailedPaymentsCount,
    int RefundedPaymentsCount,
    int CancelledPaymentsCount,
    int RejectedPaymentsCount,

    decimal PendingPaymentsAmount,
    decimal UnderReviewPaymentsAmount
);