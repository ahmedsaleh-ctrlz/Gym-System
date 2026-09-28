using Gym.Domain.Common.Result;
namespace Gym.Domain.Notifications
{
    public static class NotificationError
    {
        public static readonly Error TitleOrMessageEmpty = Error.Validation("Notification.Validation", "Cannot create notification with empty message or title");
        public static readonly Error MessageAlreadyReaded = Error.Validation("Notification.Conflict", "MessageAlreadyReaded");
    }
}
