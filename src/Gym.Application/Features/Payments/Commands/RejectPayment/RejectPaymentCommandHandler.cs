using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.RejectPayment;

public sealed class RejectPaymentCommandHandler(
    IAppDbContext dbContext,
    ILogger<RejectPaymentCommandHandler> logger,
    HybridCache cache)
    : IRequestHandler<RejectPaymentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        RejectPaymentCommand request,
        CancellationToken ct)
    {
        var payment = await dbContext.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(
                p => p.Id == request.PaymentId,
                ct);

        if (payment is null)
        {
            logger.LogWarning(
                "Payment with id {PaymentId} not found.",
                request.PaymentId);

            return ApplicationErrors.PaymentNotFound;
        }

        var result = payment.Reject();

        if (result.IsError)
        {
            logger.LogWarning(
                "Failed to reject payment with id {PaymentId}. Errors: {Errors}",
                request.PaymentId,
                string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description)));

            return result.Errors;
        }

        await dbContext.SaveChangesAsync(ct);

        await RemoveCaches(ct);

        logger.LogInformation(
            "Payment with id {PaymentId} rejected successfully.",
            request.PaymentId);

        return Result.Updated;
    }

    private async Task RemoveCaches(CancellationToken ct)
    {
        await cache.RemoveByTagAsync("Payments", ct);
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await cache.RemoveByTagAsync("AdminDashboard", ct);
        await cache.RemoveByTagAsync("AdminDashboard:PaymentsSummary", ct);
        await cache.RemoveByTagAsync("AdminDashboard:RevenueSummary", ct);
        await cache.RemoveByTagAsync("AdminDashboard:SubscriptionsSummary", ct);
        await cache.RemoveByTagAsync("AdminDashboard:Overview", ct);
    }
}
