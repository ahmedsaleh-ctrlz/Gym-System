using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Notifications.Enums;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Payments.Invoices;
using Gym.Domain.PromoCodes.PromoCodeUsage;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.ApprovePayment;

public sealed class ApprovePaymentCommandHandler(
    IAppDbContext dbContext,
    ILogger<ApprovePaymentCommandHandler> logger,
    HybridCache cache,
    IIdentityService identityService,
    INotificationService notificationService)
    : IRequestHandler<ApprovePaymentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        ApprovePaymentCommand request,
        CancellationToken ct)
    {
        var payment = await dbContext.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Member)
                    .ThenInclude(m => m.Person)
            .Include(p => p.PromoCode)
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

        if (payment.Status != PaymentStatus.UnderReview)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        var approveResult = payment.Approve();

        if (approveResult.IsError)
        {
            logger.LogWarning(
                "Failed to approve payment with id {PaymentId}. Errors: {Errors}",
                request.PaymentId,
                string.Join(
                    ", ",
                    approveResult.Errors.Select(e => e.Description)));

            return approveResult.Errors;
        }

        var subscriptionResult =
            payment.Subscription.StartDate >
            DateOnly.FromDateTime(DateTime.UtcNow)
                ? payment.Subscription.Scheduled()
                : payment.Subscription.Activate();

        if (subscriptionResult.IsError)
        {
            logger.LogWarning(
                "Failed to update subscription for payment with id {PaymentId}. Errors: {Errors}",
                request.PaymentId,
                string.Join(
                    ", ",
                    subscriptionResult.Errors.Select(e => e.Description)));

            return subscriptionResult.Errors;
        }

        if (payment.PromoCode is not null)
        {
            var recordError = RecordPromoCodeUsage(
                payment.PromoCode,
                payment);

            if (recordError is not null)
            {
                return recordError;
            }
        }

        var invoiceResult = CreateInvoice(payment);

        if (invoiceResult.IsError)
        {
            logger.LogError(
                "Cannot create invoice for payment {PaymentId}. Error: {Error}",
                payment.Id,
                invoiceResult.TopError);

            return invoiceResult.Errors;
        }

        await dbContext.SaveChangesAsync(ct);

        await RemoveCaches(ct);

        await SendNotificationToUser(payment, ct);

        logger.LogInformation(
            "Payment with id {PaymentId} approved successfully and subscription updated.",
            request.PaymentId);

        return Result.Updated;
    }

    private Error? RecordPromoCodeUsage(
        Domain.PromoCodes.PromoCode promoCode,
        Payment payment)
    {
        var usageResult = promoCode.RecordUsage();

        if (usageResult.IsError)
        {
            return usageResult.TopError;
        }

        var usage = PromoCodeUsage.Create(
            promoCode.Id,
            payment.Subscription.MemberId);

        if (usage.IsError)
        {
            return usage.TopError;
        }

        dbContext.PromoCodeUsages.Add(usage.Value);

        return null;
    }

    private Result<Invoice> CreateInvoice(Payment payment)
    {
        var invoiceNumber =
            $"INV-{DateTime.UtcNow.Year}-{payment.Id:D6}";

        var invoiceResult = Invoice.Create(
            invoiceNumber,
            payment);

        if (invoiceResult.IsError)
        {
            return invoiceResult.Errors;
        }

        dbContext.Invoices.Add(invoiceResult.Value);

        return invoiceResult.Value;
    }

    private async Task SendNotificationToUser(
        Payment payment,
        CancellationToken ct)
    {
        var personId = await dbContext.Members
            .Where(m => m.Id == payment.Subscription.MemberId)
            .Select(m => m.PersonId)
            .FirstOrDefaultAsync(ct);

        var userIdResult = await identityService
            .GetUserIdByPersonIdAsync(personId, ct);

        if (userIdResult.IsSuccess)
        {
            var message =
                payment.Subscription.Status == SubscriptionStatus.Scheduled
                    ? "Your payment has been approved successfully. Your subscription is scheduled to start on its start date."
                    : "Your payment has been approved successfully. Your subscription is now active.";

            await notificationService.SendNotificationAsync(
                userIdResult.Value,
                "Payment Approved",
                message,
                NotificationType.Payment,
                cancellationToken: ct);
        }
    }

    private async Task RemoveCaches(CancellationToken ct)
    {
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await cache.RemoveByTagAsync("Attendance", ct);
        await cache.RemoveByTagAsync("AdminDashboard:Overview", ct);
        await cache.RemoveByTagAsync("AdminDashboard:SubscriptionsSummary", ct);
        await cache.RemoveByTagAsync("AdminDashboard:PaymentsSummary", ct);
        await cache.RemoveByTagAsync("AdminDashboard:RevenuesSummary", ct);
        await cache.RemoveByTagAsync("AdminDashboard:RevenueSummary", ct);
        await cache.RemoveByTagAsync("Payments", ct);
        await cache.RemoveByTagAsync("PromoCodes", ct);
    }
}
