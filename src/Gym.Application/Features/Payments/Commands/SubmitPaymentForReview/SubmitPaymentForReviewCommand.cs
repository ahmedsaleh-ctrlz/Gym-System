using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.SubmitPaymentForReview
{
    public sealed record SubmitPaymentForReviewCommand(
    int PaymentId,
    PaymentMethod PaymentMethod,
    string PaymentReference,
    int? PromoCodeId)
    : IRequest<Result<Updated>>;

    public class SubmitPaymentForReviewCommandHandler(
        IAppDbContext dbContext,
        IUser user,
        ILogger<SubmitPaymentForReviewCommandHandler> logger,
        HybridCache cache) : IRequestHandler<SubmitPaymentForReviewCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(SubmitPaymentForReviewCommand request, CancellationToken ct)
        {
            var payment = await dbContext.Payments
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Member)
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, ct);

            if (payment is null)
            {
                logger.LogInformation("Payment with Id {PaymentId} Not Found", request.PaymentId);
                return ApplicationErrors.PaymentNotFound;
            }

            if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(user.PersonId) ||
                    !int.TryParse(user.PersonId, out var callerPersonId) ||
                    payment.Subscription?.Member?.PersonId != callerPersonId)
                {
                    logger.LogWarning("Forbidden attempt to submit payment review for PaymentId {PaymentId} by PersonId {PersonId}.", request.PaymentId, user.PersonId);
                    return ApplicationErrors.InvalidPaymentOwner;
                }
            }

            PromoCode? promoCode = null;

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

            var result = payment.SubmitForReview(
                request.PaymentMethod,
                promoCode,
                request.PaymentReference);

            if (result.IsError)
            {
                return result.Errors;
            }

            await dbContext.SaveChangesAsync(ct);

            return Result.Updated;
        }

        private async Task<Result<PromoCode>> ValidatePromoCode(
            Payment payment,
            int promoCodeId, CancellationToken ct)
        {
            var promoCode = await dbContext.PromoCodes
                .FirstOrDefaultAsync(
                    p => p.Id == promoCodeId);

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
    }
}
