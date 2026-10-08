using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PaymentsSummary;

public sealed class GetAdminPaymentsSummaryQueryHandler(
    IAppDbContext dbContext)
    : IRequestHandler<
        GetAdminPaymentsSummaryQuery,
        Result<AdminPaymentsSummaryResponse>>
{
    public async Task<Result<AdminPaymentsSummaryResponse>> Handle(
        GetAdminPaymentsSummaryQuery request,
        CancellationToken ct)
    {
        var pendingPaymentsCount =
            await dbContext.Payments.CountAsync(
                p => p.Status == PaymentStatus.Pending,
                ct);

        var underReviewPaymentsCount =
            await dbContext.Payments.CountAsync(
                p => p.Status == PaymentStatus.UnderReview,
                ct);

        var paidPaymentsCount =
            await dbContext.Payments.CountAsync(
                p => p.Status == PaymentStatus.Paid,
                ct);

        var failedPaymentsCount =
            await dbContext.Payments.CountAsync(
                p => p.Status == PaymentStatus.Failed,
                ct);

        var refundedPaymentsCount =
            await dbContext.Payments.CountAsync(
                p => p.Status == PaymentStatus.Refunded,
                ct);

        var cancelledPaymentsCount =
            await dbContext.Payments.CountAsync(
                p => p.Status == PaymentStatus.Cancelled,
                ct);

        var rejectedPaymentsCount =
            await dbContext.Payments.CountAsync(
                p => p.Status == PaymentStatus.Rejected,
                ct);

        var pendingPaymentsAmount =
            await dbContext.Payments
                .Where(p => p.Status == PaymentStatus.Pending)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var underReviewPaymentsAmount =
            await dbContext.Payments
                .Where(p => p.Status == PaymentStatus.UnderReview)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

        var response = new AdminPaymentsSummaryResponse(
            PendingPaymentsCount: pendingPaymentsCount,
            UnderReviewPaymentsCount: underReviewPaymentsCount,
            PaidPaymentsCount: paidPaymentsCount,
            FailedPaymentsCount: failedPaymentsCount,
            RefundedPaymentsCount: refundedPaymentsCount,
            CancelledPaymentsCount: cancelledPaymentsCount,
            RejectedPaymentsCount: rejectedPaymentsCount,
            PendingPaymentsAmount: pendingPaymentsAmount,
            UnderReviewPaymentsAmount: underReviewPaymentsAmount);

        return response;
    }
}