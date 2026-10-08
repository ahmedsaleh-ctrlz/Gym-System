using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.RefundPayment
{
    public record RefundPaymentCommand(int PaymentId) : IRequest<Result<Updated>>;
    public class RefundPaymentCommandHandler(ILogger<RefundPaymentCommand> logger, IAppDbContext dbContext, HybridCache cache) : IRequestHandler<RefundPaymentCommand, Result<Updated>>
    {
        public async Task<Result<Updated>> Handle(RefundPaymentCommand request, CancellationToken ct)
        {
            logger.LogTrace("Handling refund payment with id {paymentId}", request.PaymentId);
            var payment = await dbContext.Payments.Include(p => p.Subscription).FirstOrDefaultAsync(p => p.Id == request.PaymentId);
            if(payment is null)
            {
                logger.LogError("Payment with id {id} cannot found", request.PaymentId);
                return ApplicationErrors.PaymentNotFound;
            }

            var refundResult = payment.Refund();

            if (refundResult.IsError)
            {
                logger.LogError("Cannot refund payment with id: {id} due to {errors}", request.PaymentId, refundResult.Errors);
                return refundResult.TopError;
            }

            logger.LogInformation("Payment with id {PaymentId} refunded successfully and subscription canceled", request.PaymentId);

            await cache.RemoveByTagAsync("Subscriptions", ct);
            await cache.RemoveByTagAsync("Attendance", ct);
            await cache.RemoveByTagAsync("AdminDashboard", ct);
            await cache.RemoveByTagAsync("Payments", ct);
            await cache.RemoveByTagAsync("AdminDashboard:PaymentsSummary", ct);
            await cache.RemoveByTagAsync("AdminDashboard:RevenueSummary", ct);
            await cache.RemoveByTagAsync("AdminDashboard:SubscriptionsSummary", ct);
            await cache.RemoveByTagAsync("AdminDashboard:Overview", ct);
            await dbContext.SaveChangesAsync(ct);
            return Result.Updated;
        }
    }
}
