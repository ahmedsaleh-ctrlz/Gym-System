using Gym.Application.Common.Interfaces.BackgroundJobsServices;
using Gym.Domain.Notifications.Enums;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class TestNotificationBackgroundJobs : INotificationBackgroundJobs
{
    public void SendToUsers(
        IEnumerable<string> userIds,
        string title,
        string message,
        NotificationType type)
    {
        return;
    }
}