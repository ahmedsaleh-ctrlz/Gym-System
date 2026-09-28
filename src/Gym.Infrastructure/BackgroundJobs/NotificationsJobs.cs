using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Interfaces.BackgroundJobsServices;
using Gym.Domain.Notifications.Enums;

using Hangfire;

using MediatR;

namespace Gym.Infrastructure.BackgroundJobs
{
    public sealed class NotificationJobs(
     INotificationService notificationService)
    {
        public async Task SendToUsersAsync(
            IEnumerable<string> userIds,
            string title,
            string message,
            NotificationType type)
        {
            var result = await notificationService.SendNotificationToUsersAsync(
                userIds,
                title,
                message,
                type);

            if (result.IsError)
            {
                throw new InvalidOperationException(
                    "Failed to send notifications.");
            }
        }
    }
}

