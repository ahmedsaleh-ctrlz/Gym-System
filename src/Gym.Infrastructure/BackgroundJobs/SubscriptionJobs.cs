using Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ActivateScheduledSubscriptions;
using Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ExpireSubscriptions;
using Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.UnfreezeSubscriptions;

using MediatR;

namespace Gym.Infrastructure.BackgroundJobs;

public sealed class SubscriptionJobs(ISender sender)
{
    public async Task ActivateScheduledSubscriptions()
    {
        await sender.Send(
            new ActivateScheduledSubscriptionsCommand());
    }

    public async Task ExpireSubscriptions()
    {
        await sender.Send(
            new ExpireSubscriptionsCommand());
    }

    public async Task UnfreezeSubscriptions()
    {
        await sender.Send(
            new UnfreezeSubscriptionsCommand());
    }
}