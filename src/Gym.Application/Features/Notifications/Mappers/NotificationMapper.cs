using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

using Gym.Application.Features.Notifications.Dtos;
using Gym.Domain.Notifications;

namespace Gym.Application.Features.Notifications.Mappers
{
    public static class NotificationMapper
    {
        public static Expression<Func<Notification, NotificationResponse>> ToResponse()
        {
            return n => new NotificationResponse(n.Id, n.Title, n.Message, n.Type, n.Data, n.IsRead, n.CreatedAt);
        }

        public static NotificationResponse ToResponse(this Notification notification)
        {
            return new NotificationResponse(
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.Data,
                notification.IsRead,
                notification.CreatedAt);
        }
    }
}
