using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Notifications.Enums;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Payments.Invoices;
using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.Enums;
using Gym.Domain.PromoCodes.PromoCodeUsage;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.PayPayment;

public sealed class PayPaymentCommandHandler(
    IAppDbContext dbContext,
    ILogger<PayPaymentCommandHandler> logger,
    HybridCache cache,
    IIdentityService identityService,
    INotificationService notificationService)
    : IRequestHandler<PayPaymentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        PayPaymentCommand request,
        CancellationToken ct)
    {
        PromoCode? promoCode = null;

        var payment = await dbContext.Payments
        .Include(p => p.PromoCode)
        .Include(p => p.Subscription)
            .ThenInclude(s => s.Plan)
        .Include(p => p.Subscription)
            .ThenInclude(s => s.Member)
                .ThenInclude(m => m.Person)
        .FirstOrDefaultAsync(
            p => p.Id == request.PaymentId,
            ct);

        if (payment is null)
        {
            logger.LogWarning(
                "Payment with id {PaymentId} not found",
                request.PaymentId);

            return ApplicationErrors.PaymentNotFound;
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return PaymentErrors.InvalidPaymentStatus;
        }

        if (request.PromoCodeId is not null)
        {
            var promoResult = await ValidatePromoCode(
                payment,
                request.PromoCodeId.Value,
                ct);

            if (promoResult.IsError)
            {
                return promoResult.Errors;
            }

            promoCode = promoResult.Value;
        }

        Result<Updated> paymentResult;

        if (promoCode is not null && request.PaymentMethod is null)
        {
            var result = payment.CompleteFreePayment(promoCode);

            if (result.IsError)
            {
                return result.Errors;
            }

            paymentResult = result;
        }
        else
        {
            paymentResult = payment.Pay(
                request.PaymentMethod!.Value,
                promoCode,
                request.PaymentReference);

            if (paymentResult.IsError)
            {
                logger.LogWarning(
                    "Failed to pay payment with id {PaymentId}. Errors: {Errors}",
                    request.PaymentId,
                    string.Join(", ", paymentResult.Errors.Select(e => e.Description)));

                return paymentResult.Errors;
            }
        }

        var subscriptionResult =
            payment.Subscription.StartDate > DateOnly.FromDateTime(DateTime.UtcNow)
                ? payment.Subscription.Scheduled()
                : payment.Subscription.Activate();

        if (subscriptionResult.IsError)
        {
            logger.LogWarning(
                "Failed to update subscription for payment with id {PaymentId}. Errors: {Errors}",
                request.PaymentId,
                string.Join(", ", subscriptionResult.Errors.Select(e => e.Description)));

            return subscriptionResult.Errors;
        }

        if (promoCode is not null)
        {
            var recordError = await RecordPromoCodeUsage(promoCode, payment);
            if(recordError is not null)
            {
                return recordError;
            }
        }

        var invoiceResult = CreateInvoice(payment);
        if (invoiceResult.IsError)
        {
            logger.LogError("Cannot Create Invoice due to {Error}", invoiceResult.TopError);
            return invoiceResult.Errors;
        }

        await dbContext.SaveChangesAsync(ct);
        await RemoveCaches(ct);
        await SendNotificationToUser(payment, ct);

        logger.LogInformation(
            "Payment with id {PaymentId} paid successfully and subscription updated",
            request.PaymentId);

        return Result.Updated;
    }

    private async Task<Result<PromoCode>> ValidatePromoCode(
    Payment payment,
    int promoCodeId,
    CancellationToken ct)
    {
        var promoCode = await dbContext.PromoCodes
            .FirstOrDefaultAsync(
                p => p.Id == promoCodeId,
                ct);

        if (promoCode is null)
        {
            return ApplicationErrors.PromoCodeNotValid;
        }

        var promoValidation = promoCode.CanBeAppliedTo(
            payment.Subscription.PlanId,
            payment.SubTotal);

        if (promoValidation is not null)
        {
            return promoValidation;
        }

        var alreadyUsed = await dbContext.PromoCodeUsages
            .AnyAsync(
                x => x.PromoCodeId == promoCode.Id &&
                     x.MemberId == payment.Subscription.MemberId,
                ct);

        if (alreadyUsed)
        {
            return ApplicationErrors.PromoCodeAlreadyUsed;
        }

        if (promoCode.Audience == PromoCodeAudience.NewMembers)
        {
            var hasPreviousSubscription = await dbContext.Subscriptions
                .AnyAsync(
                    s => s.MemberId == payment.Subscription.MemberId &&
                         s.Id != payment.SubscriptionId,
                    ct);

            if (hasPreviousSubscription)
            {
                return ApplicationErrors.PromoCodeOnlyForNewMembers;
            }
        }

        return promoCode;
    }

    private async Task<Error?> RecordPromoCodeUsage(PromoCode promoCode, Payment payment)
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
    private async Task SendNotificationToUser(Payment payment, CancellationToken ct )
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
                    ? "Your payment has been completed successfully. Your subscription is scheduled to start on its start date."
                    : "Your payment has been completed successfully. Your subscription is now active.";

            await notificationService.SendNotificationAsync(
                userIdResult.Value,
                "Payment Completed",
                message,
                NotificationType.Payment,
                cancellationToken: ct);
        }
    }
    private async Task RemoveCaches(CancellationToken ct)
    {
        await cache.RemoveByTagAsync("Subscriptions", ct);
        await cache.RemoveByTagAsync("Attendance", ct);
        await cache.RemoveByTagAsync("AdminDashboard", ct);
        await cache.RemoveByTagAsync("Payments", ct);
        await cache.RemoveByTagAsync("PromoCodes", ct);
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
}