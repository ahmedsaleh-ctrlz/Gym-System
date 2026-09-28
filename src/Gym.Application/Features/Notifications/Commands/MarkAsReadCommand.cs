using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Notifications.Commands
{
    public sealed record MarkAsReadCommand(
    int NotificationId,
    string UserId) : IRequest<Result<Updated>>;

    public sealed class MarkAsReadCommandHandler(
    INotificationService notificationService)
    : IRequestHandler<MarkAsReadCommand, Result<Updated>>
    {
        public Task<Result<Updated>> Handle(
            MarkAsReadCommand request,
            CancellationToken cancellationToken)
        {
            return notificationService.MarkAsReadAsync(
                request.NotificationId,
                request.UserId,
                cancellationToken);
        }
    }
}
