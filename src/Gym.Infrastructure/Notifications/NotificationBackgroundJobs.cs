using Gym.Application.Common.Interfaces.BackgroundJobsServices;
using Gym.Domain.Notifications.Enums;
using Gym.Infrastructure.BackgroundJobs;

using Hangfire;

public sealed class NotificationBackgroundJobs(
    IBackgroundJobClient backgroundJobClient)
    : INotificationBackgroundJobs
{
    public void SendToUsers(
        IEnumerable<string> userIds,
        string title,
        string message,
        NotificationType type)
    {
        backgroundJobClient.Enqueue<NotificationJobs>(
            job => job.SendToUsersAsync(
                userIds,
                title,
                message,
                type));
    }
}