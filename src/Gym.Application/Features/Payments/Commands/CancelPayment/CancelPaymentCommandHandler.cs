using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Notifications.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Commands.CancelPayment;

public sealed record CancelPaymentCommandHandler(
    IAppDbContext DbContext,
    ILogger<CancelPaymentCommand> Logger,
    HybridCache Cache,
    IIdentityService IdentityService,
    INotificationService NotificationService)
    : IRequestHandler<CancelPaymentCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        CancelPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var payment = await DbContext.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(
                p => p.Id == request.PaymentId,
                cancellationToken);

        if (payment is null)
        {
            Logger.LogWarning(
                "Payment with id {PaymentId} not found",
                request.PaymentId);

            return ApplicationErrors.PaymentNotFound;
        }

        var result = payment.Cancel();

        if (result.IsError)
        {
            Logger.LogWarning(
                "Payment with id {PaymentId} cannot be cancelled. Status: {Status}",
                request.PaymentId,
                payment.Status);

            return result.Errors;
        }

        await DbContext.SaveChangesAsync(cancellationToken);

        await Cache.RemoveByTagAsync("Payments", cancellationToken);
        await Cache.RemoveByTagAsync("Subscriptions", cancellationToken);
        await Cache.RemoveByTagAsync("AdminDashboard:PaymentsSummary", cancellationToken);
        await Cache.RemoveByTagAsync("AdminDashboard:RevenueSummary", cancellationToken);
        await Cache.RemoveByTagAsync("AdminDashboard:SubscriptionsSummary", cancellationToken);
        await Cache.RemoveByTagAsync("AdminDashboard:Overview", cancellationToken);

        var personId = await DbContext.Members
            .Where(m => m.Id == payment.Subscription.MemberId)
            .Select(m => m.PersonId)
            .FirstOrDefaultAsync(cancellationToken);

        var userIdResult = await IdentityService
            .GetUserIdByPersonIdAsync(personId, cancellationToken);

        if (userIdResult.IsSuccess)
        {
            await NotificationService.SendNotificationAsync(
                userIdResult.Value,
                "Payment Cancelled",
                "Your payment has been cancelled. Please review your payment details and try again.",
                NotificationType.Payment,
                cancellationToken: cancellationToken);
        }

        Logger.LogInformation(
            "Payment with id {PaymentId} cancelled successfully",
            request.PaymentId);

        return Result.Updated;
    }
}
