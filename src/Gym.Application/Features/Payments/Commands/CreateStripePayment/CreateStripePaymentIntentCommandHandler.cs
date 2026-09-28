using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.PromoCodes.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.CreateStripePayment;

public sealed class CreateStripePaymentIntentCommandHandler(
    IAppDbContext dbContext,
    IStripePaymentService stripePaymentService,
    ILogger<CreateStripePaymentIntentCommandHandler> logger)
    : IRequestHandler<
        CreateStripePaymentIntentCommand,
        Result<StripePaymentIntentResult>>
{
    public async Task<Result<StripePaymentIntentResult>> Handle(
        CreateStripePaymentIntentCommand request,
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

        if (payment.Status != PaymentStatus.Pending)
        {
            logger.LogWarning(
                "Payment with id {PaymentId} is not pending.",
                request.PaymentId);

            return PaymentErrors.InvalidPaymentStatus;
        }

        var amount = payment.Amount;
        int? promoCodeId = null;

        if (request.PromoCodeId.HasValue)
        {
            var promoCode = await dbContext.PromoCodes
                .FirstOrDefaultAsync(
                    p => p.Id == request.PromoCodeId.Value,
                    ct);

            if (promoCode is null)
            {
                return ApplicationErrors.PromoCodeNotValid;
            }

            var promoError = promoCode.CanBeAppliedTo(
                payment.Subscription.PlanId,
                payment.SubTotal);

            if (promoError is not null)
            {
                return promoError;
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
                var hasPreviousSubscription =
                    await dbContext.Subscriptions
                        .AnyAsync(
                            s => s.MemberId == payment.Subscription.MemberId &&
                                 s.Id != payment.SubscriptionId,
                            ct);

                if (hasPreviousSubscription)
                {
                    return ApplicationErrors.PromoCodeOnlyForNewMembers;
                }
            }

            var discount = promoCode.CalculateDiscount(
                payment.SubTotal);

            amount = Math.Max(
                0,
                payment.SubTotal - discount + payment.Tax);

            promoCodeId = promoCode.Id;
        }

        if (amount <= 0)
        {
            return PaymentErrors.PaymentAmountMustBeGreaterThanZero;
        }

        var metadata = promoCodeId.HasValue
            ? new Dictionary<string, string>
            {
                ["promoCodeId"] = promoCodeId.Value.ToString()
            }
            : new Dictionary<string, string>();

        StripePaymentIntentResult stripeResult;

        if (!string.IsNullOrWhiteSpace(payment.ExternalTransactionId))
        {
            logger.LogInformation(
                "Updating existing Stripe PaymentIntent {PaymentIntentId} for payment {PaymentId}. PromoCodeId: {PromoCodeId}",
                payment.ExternalTransactionId,
                payment.Id,
                promoCodeId);

            stripeResult =
                await stripePaymentService.UpdatePaymentIntentAsync(
                    payment.ExternalTransactionId,
                    amount,
                    metadata,
                    ct);
        }
        else
        {
            stripeResult =
                await stripePaymentService.CreatePaymentIntentAsync(
                    amount,
                    metadata,
                    ct);

            payment.SetExternalTransactionId(
                stripeResult.PaymentIntentId);

            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation(
                "Stripe PaymentIntent {PaymentIntentId} created for payment {PaymentId}. PromoCodeId: {PromoCodeId}",
                stripeResult.PaymentIntentId,
                payment.Id,
                promoCodeId);
        }

        return stripeResult;
    }
}