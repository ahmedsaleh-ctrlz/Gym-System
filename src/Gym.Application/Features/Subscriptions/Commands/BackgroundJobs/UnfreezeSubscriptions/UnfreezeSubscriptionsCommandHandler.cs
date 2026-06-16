using Gym.Application.Common.Interfaces;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.UnfreezeSubscriptions;

public sealed class UnfreezeSubscriptionsCommandHandler(IAppDbContext dbContext) : IRequestHandler<UnfreezeSubscriptionsCommand>
{
    public async Task Handle(UnfreezeSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        var frozenSubscriptions = await dbContext.Subscriptions.Where(s => s.Status == SubscriptionStatus.Frozen)
            .ToListAsync(cancellationToken);

        foreach (var subscription in frozenSubscriptions)
        {
            if (subscription.CanUnFreeze())
            {
                subscription.Unfreeze();
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}