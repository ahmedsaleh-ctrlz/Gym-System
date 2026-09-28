using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Notifications.Enums;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Identity.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(
    string UserId,
    string Token)
    : IRequest<Result<Updated>>;

public sealed class ConfirmEmailCommandHandler(
    IIdentityService identityService,
    HybridCache cache,
    ILogger<ConfirmEmailCommandHandler> logger, INotificationService notificationService)
    : IRequestHandler<ConfirmEmailCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        ConfirmEmailCommand request,
        CancellationToken ct)
    {
        var result = await identityService.ConfirmEmailAsync(
            request.UserId,
            request.Token);

        if (result.IsError)
        {
            logger.LogWarning(
                "Failed to confirm email for user {UserId}",
                request.UserId);

            return result.Errors;
        }

        await notificationService.SendNotificationAsync(request.UserId, "Email Confirmed Successfully", "Thanks for confirm your email. Get started with us now!", NotificationType.General, cancellationToken: ct);
        await cache.RemoveByTagAsync("Member", ct);
        await cache.RemoveByTagAsync("AdminDashboard", ct);

        logger.LogInformation(
            "Email confirmed successfully for user {UserId}",
            request.UserId);

        return Result.Updated;
    }
}