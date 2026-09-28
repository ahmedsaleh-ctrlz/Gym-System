using System;
using System.Collections.Generic;
using System.Text;

using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Notifications.Enums;

namespace Gym.Domain.Notifications
{
    public class Notification : Entity
    {
        public string UserId { get; private set; }

        public string Title { get; private set; } = null!;

        public string Message { get; private set; } = null!;

        public NotificationType Type { get; private set; }

        public string? Data { get; private set; }

        public bool IsRead { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? ReadAt { get; private set; }

        private Notification()
        {
        }

        private Notification(
        string userId,
        string title,
        string message,
        NotificationType type,
        string? data)
        {
            UserId = userId;
            Title = title;
            Message = message;
            Type = type;
            Data = data;
            IsRead = false;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<Notification> Create(
        string userId,
        string title,
        string message,
        NotificationType type,
        string? data = null)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
            {
                return NotificationError.TitleOrMessageEmpty;
            }

            return new Notification(
            userId,
            title,
            message,
            type,
            data);
        }

        public Result<Updated> MarkAsRead()
        {
            if (IsRead)
            {
                return NotificationError.MessageAlreadyReaded;
            }

            ReadAt = DateTime.UtcNow;
            IsRead = true;
            return Result.Updated;
        }
    }
}
