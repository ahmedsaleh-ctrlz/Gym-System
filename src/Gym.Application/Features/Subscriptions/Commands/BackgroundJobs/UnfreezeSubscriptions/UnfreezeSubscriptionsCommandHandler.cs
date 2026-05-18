using Gym.Application.Common.Interfaces;
using Gym.Domain.Subscriptions.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.UnfreezeSubscriptions;

public sealed class UnfreezeSubscriptionsCommandHandler(IAppDbContext dbContext) : IRequestHandler<UnfreezeSubscriptionsCommand>
{
    public async Task Handle(UnfreezeSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        await dbContext.Subscriptions.Where(s => s.Status == SubscriptionStatus.Frozen && s.CanUnFreeze())
            .ForEachAsync(s => s.Unfreeze(), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
