using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.CancelPayment;

public sealed record CancelPaymentCommandHandler(IAppDbContext DbContext, ILogger<CancelPaymentCommand> Logger, HybridCache Cache) : IRequestHandler<CancelPaymentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await DbContext.Payments.Include(p => p.Subscription).FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);
        if(payment is null)
        {
            Logger.LogWarning("Payment with id {PaymentId} not found", request.PaymentId);
            return ApplicationErrors.PaymentNotFound;
        }

        var result = payment.Cancel();
        if(result.IsError)
        {
            Logger.LogWarning("Payment with id {PaymentId} cannot be cancelled. Status: {Status}", request.PaymentId, payment.Status);
            return result.Errors;
        }

        Logger.LogInformation("Payment with id {PaymentId} cancelled successfully", request.PaymentId);
        await Cache.RemoveByTagAsync("Payments");
        await Cache.RemoveByTagAsync("Subscriptions");
        await DbContext.SaveChangesAsync(cancellationToken);
        return Result.Updated;
    }
}
