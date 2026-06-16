using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.PayPayment;

public sealed class PayPaymentCommandHandler(IAppDbContext dbContext, ILogger<PayPaymentCommandHandler> logger, HybridCache cache) : IRequestHandler<PayPaymentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(PayPaymentCommand request, CancellationToken ct)
    {
        var payment = await dbContext.Payments.Include(p => p.Subscription).FirstOrDefaultAsync(p => p.Id == request.PaymentId, ct);
        if (payment is null)
        {
            logger.LogWarning("Payment with id {PaymentId} not found", request.PaymentId);
            return ApplicationErrors.PaymentNotFound;
        }

        var payResult = payment.Pay(request.PaymentMethod);
        if (payResult.IsError)
        {
            logger.LogWarning("Failed to pay payment with id {PaymentId}. Errors: {Errors}", request.PaymentId, string.Join(", ", payResult.Errors.Select(e => e.Description)));
            return payResult.Errors;
        }

        var subscriptionResult = payment.Subscription.StartDate > DateOnly.FromDateTime(DateTime.UtcNow) ? payment.Subscription.Scheduled() : payment.Subscription.Activate();

        if (subscriptionResult.IsError)
        {
            logger.LogWarning("Failed to update subscription for payment with id {PaymentId}. Errors: {Errors}", request.PaymentId, string.Join(", ", subscriptionResult.Errors.Select(e => e.Description)));
            return subscriptionResult.Errors;
        }

        logger.LogInformation("Payment with id {PaymentId} paid successfully and subscription updated", request.PaymentId);

        await cache.RemoveByTagAsync("Subscriptions", ct);
        await cache.RemoveByTagAsync("AdminDashboard", ct);
        await cache.RemoveByTagAsync("Payments", ct);
        await dbContext.SaveChangesAsync(ct);
        return Result.Updated;
    }
}