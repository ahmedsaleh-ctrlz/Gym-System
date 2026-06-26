using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.CreateStripePayment;
public sealed class CreateStripePaymentIntentCommandHandler(
    IAppDbContext dbContext,
    IStripePaymentService stripePaymentService,
    ILogger<CreateStripePaymentIntentCommandHandler> logger)
    : IRequestHandler<CreateStripePaymentIntentCommand, Result<StripePaymentIntentResult>>
{
    public async Task<Result<StripePaymentIntentResult>> Handle(
        CreateStripePaymentIntentCommand request,
        CancellationToken ct)
    {
        var payment = await dbContext.Payments
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

        if (!string.IsNullOrWhiteSpace(payment.ExternalTransactionId))
        {
            logger.LogWarning(
                "Payment with id {PaymentId} already has a Stripe PaymentIntent.",
                request.PaymentId);

            return PaymentErrors.InvalidPaymentStatus;
        }

        var stripeResult =
            await stripePaymentService.CreatePaymentIntentAsync(
                payment.Amount,
                ct);

        payment.SetExternalTransactionId(
            stripeResult.PaymentIntentId);

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "Stripe PaymentIntent {PaymentIntentId} created for payment {PaymentId}.",
            stripeResult.PaymentIntentId,
            payment.Id);

        return stripeResult;
    }
}