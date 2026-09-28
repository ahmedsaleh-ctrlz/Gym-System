using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Notifications.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Notifications.Queries.GetNotificationsQuery
{
    public sealed class GetNotificationsQueryHandler(
    INotificationService notificationService)
    : IRequestHandler<
        GetNotificationsQuery,
        Result<PaginatedList<NotificationResponse>>>
    {
        public Task<Result<PaginatedList<NotificationResponse>>> Handle(
            GetNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            return notificationService.GetNotificationsAsync(
                request.UserId,
                request.PageNumber,
                request.PageSize,
                request.IsRead,
                cancellationToken);
        }
    }
}
