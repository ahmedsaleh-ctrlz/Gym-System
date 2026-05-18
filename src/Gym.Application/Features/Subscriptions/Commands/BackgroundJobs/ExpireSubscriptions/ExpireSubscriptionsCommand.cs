using MediatR;

namespace Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ExpireSubscriptions;

public sealed record ExpireSubscriptionsCommand : IRequest;
