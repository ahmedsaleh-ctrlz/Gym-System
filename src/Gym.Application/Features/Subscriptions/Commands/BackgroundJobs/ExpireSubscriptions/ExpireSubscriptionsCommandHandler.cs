using Gym.Application.Common.Interfaces;
using Gym.Domain.Subscriptions.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ExpireSubscriptions;

public sealed class ExpireSubscriptionsCommandHandler(IAppDbContext dbContext) : IRequestHandler<ExpireSubscriptionsCommand>
{
    public async Task Handle(ExpireSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        await dbContext.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active && s.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
            .ForEachAsync(s => s.Expire(), cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}