using Gym.Domain.Common.Result;
using Gym.Domain.Notifications;
using Gym.Domain.Notifications.Enums;

namespace Gym.Tests.Common.Notifications;

public static class NotificationFactory
{
    public static Result<Notification> CreateNotification(
        string userId = "user-test-id",
        string title = "Welcome",
        string message = "Welcome to the Gym System",
        NotificationType type = NotificationType.General,
        string? data = null)
    {
        return Notification.Create(
            userId,
            title,
            message,
            type,
            data);
    }
}
