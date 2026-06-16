using Gym.Application.Common.Interfaces;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ActivateScheduledSubscriptions;

public sealed class ActivateScheduledSubscriptionsCommandHandler(IAppDbContext dbContext, ILogger<ActivateScheduledSubscriptionsCommandHandler> logger) : IRequestHandler<ActivateScheduledSubscriptionsCommand>
{
    public async Task Handle(ActivateScheduledSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        var subscriptions = await dbContext.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Scheduled && s.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                .ToListAsync(cancellationToken);

        foreach (var sub in subscriptions)
        {
            var result = sub.Activate();
            if (result.IsError)
            {
                logger.LogError("Failed to activate subscription {SubscriptionId}: {Error}", sub.Id, result.Errors);
                continue;
            }

            logger.LogInformation("Activated subscription {SubscriptionId}", sub.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}