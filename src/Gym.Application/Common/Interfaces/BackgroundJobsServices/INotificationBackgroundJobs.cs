using System;
using System.Collections.Generic;
using System.Text;

using Gym.Domain.Notifications.Enums;

namespace Gym.Application.Common.Interfaces.BackgroundJobsServices
{
    public interface INotificationBackgroundJobs
    {
        void SendToUsers(
        IEnumerable<string> userIds,
        string title,
        string message,
        NotificationType type);
    }
}
