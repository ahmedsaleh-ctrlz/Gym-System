using Gym.Application.Common.Interfaces;
using Gym.Domain.Notifications.Enums;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ExpireSubscriptions;

public sealed class ExpireSubscriptionsCommandHandler(
    IAppDbContext dbContext,
    ILogger<ExpireSubscriptionsCommandHandler> logger,
    INotificationService notificationService,
    IIdentityService identityService)
    : IRequestHandler<ExpireSubscriptionsCommand>
{
    public async Task Handle(
        ExpireSubscriptionsCommand request,
        CancellationToken cancellationToken)
    {
        var subscriptions = await dbContext.Subscriptions
            .Where(s =>
                s.Status == SubscriptionStatus.Active &&
                s.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
            .ToListAsync(cancellationToken);

        var memberIds = new List<int>();

        foreach (var subscription in subscriptions)
        {
            var result = subscription.Expire();

            if (result.IsError)
            {
                logger.LogError(
                    "Failed to expire subscription {SubscriptionId}: {Errors}",
                    subscription.Id,
                    result.Errors);

                continue;
            }

            memberIds.Add(subscription.MemberId);

            logger.LogInformation(
                "Expired subscription {SubscriptionId}",
                subscription.Id);
        }

        if (memberIds.Count == 0)
        {
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var personIds = await dbContext.Members
            .Where(m => memberIds.Contains(m.Id))
            .Select(m => m.PersonId)
            .ToListAsync(cancellationToken);

        var userIdsResult = await identityService
            .GetUsersIdsByPersonIdsAsync(
                personIds,
                cancellationToken);

        if (userIdsResult.IsError)
        {
            logger.LogError(
                "Failed to get user ids for expired subscriptions.");

            return;
        }

        if (userIdsResult.Value.Any())
        {
            await notificationService.SendNotificationToUsersAsync(
                userIdsResult.Value,
                "Subscription Expired",
                "Your subscription has expired. Please renew your subscription to continue using your membership.",
                NotificationType.SubscriptionStatusChanged,
                cancellationToken: cancellationToken);
        }
    }
}