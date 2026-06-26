using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Commands.PayPayment;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.ProcessStripePayment;

public sealed class ProcessStripePaymentCommandHandler(
    IAppDbContext dbContext,
    ISender sender,
    ILogger<ProcessStripePaymentCommandHandler> logger)
    : IRequestHandler<ProcessStripePaymentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        ProcessStripePaymentCommand request,
        CancellationToken ct)
    {
        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(
                p => p.ExternalTransactionId == request.PaymentIntentId,
                ct);

        if (payment is null)
        {
            logger.LogWarning(
                "Payment with PaymentIntentId {PaymentIntentId} not found.",
                request.PaymentIntentId);

            return ApplicationErrors.PaymentNotFound;
        }

        var result = await sender.Send(
            new PayPaymentCommand(
                payment.Id,
                PaymentMethod.Visa),
            ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "Failed to process Stripe payment for PaymentId {PaymentId}. Errors: {Errors}",
                payment.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return result.Errors;
        }

        logger.LogInformation(
            "Stripe payment processed successfully for PaymentId {PaymentId}.",
            payment.Id);

        return Result.Updated;
    }
}