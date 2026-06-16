using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ActivateScheduledSubscriptions;

public sealed record ActivateScheduledSubscriptionsCommand : IRequest;