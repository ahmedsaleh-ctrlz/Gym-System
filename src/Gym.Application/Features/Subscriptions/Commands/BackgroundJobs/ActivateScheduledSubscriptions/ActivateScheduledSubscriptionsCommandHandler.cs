using Gym.Application.Common.Interfaces;
using Gym.Domain.Notifications.Enums;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ActivateScheduledSubscriptions;

public sealed class ActivateScheduledSubscriptionsCommandHandler(IAppDbContext dbContext, ILogger<ActivateScheduledSubscriptionsCommandHandler> logger, INotificationService notificationService, IIdentityService identityService) : IRequestHandler<ActivateScheduledSubscriptionsCommand>
{
    public async Task Handle(ActivateScheduledSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        var subscriptions = await dbContext.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Scheduled && s.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                .ToListAsync(cancellationToken);
        var memberIds = new List<int>();
        foreach (var sub in subscriptions)
        {
            var result = sub.Activate();
            if (result.IsError)
            {
                logger.LogError("Failed to activate subscription {SubscriptionId}: {Error}", sub.Id, result.Errors);
                continue;
            }

            memberIds.Add(sub.MemberId);

            logger.LogInformation("Activated subscription {SubscriptionId}", sub.Id);
        }

        List<int> PersonIds = await dbContext.Members.Where(m => memberIds.Contains(m.Id)).Select(m => m.PersonId).ToListAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var userIds = await identityService.GetUsersIdsByPersonIdsAsync(PersonIds);
        if (userIds.Value.Count() > 0)
        {
            await notificationService.SendNotificationToUsersAsync(
                userIds.Value,
                "Subscription Activated",
                "Your subscription is now active. You can start using your membership.",
                NotificationType.SubscriptionStatusChanged,
                cancellationToken: cancellationToken);
        }
    }
}