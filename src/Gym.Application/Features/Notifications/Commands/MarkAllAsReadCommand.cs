using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Notifications.Commands
{
    public sealed record MarkAllAsReadCommand(
    string UserId) : IRequest<Result<Updated>>;
    public sealed class MarkAllAsReadCommandHandler(
    INotificationService notificationService)
    : IRequestHandler<MarkAllAsReadCommand, Result<Updated>>
    {
        public Task<Result<Updated>> Handle(
            MarkAllAsReadCommand request,
            CancellationToken cancellationToken)
        {
            return notificationService.MarkAllAsReadAsync(
                request.UserId,
                cancellationToken);
        }
    }
}
