using Gym.Application.Common.Interfaces;
using Gym.Domain.Subscriptions.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ActivateScheduledSubscriptions;

public sealed class ActivateScheduledSubscriptionsCommandHandler(IAppDbContext dbContext) : IRequestHandler<ActivateScheduledSubscriptionsCommand>
{
    public async Task Handle(ActivateScheduledSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        await dbContext.Subscriptions.Where(s=> s.Status == SubscriptionStatus.Scheduled && s.StartDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            .ForEachAsync(s => s.Activate(), cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

