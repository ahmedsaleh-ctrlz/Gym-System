using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Notifications.Dtos;
using Gym.Application.Features.Notifications.Mappers;
using Gym.Domain.Common.Result;
using Gym.Domain.Notifications;
using Gym.Domain.Notifications.Enums;
using Gym.Infrastructure.Hubs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Gym.Infrastructure.Notifications
{
    public class NotificaitonService(IAppDbContext dbContext, HybridCache cache, IHubContext<NotificationHub> hubContext) : INotificationService
    {
        public async Task<Result<PaginatedList<NotificationResponse>>> GetNotificationsAsync(string userId, int pageNumber, int pageSize, bool? isRead, CancellationToken cancellationToken = default)
        {
            var notificaionsQuery = dbContext.Notifications.AsNoTracking().Where(n => n.UserId == userId);
            if (isRead.HasValue)
            {
                notificaionsQuery = notificaionsQuery.Where(n => n.IsRead == isRead.Value);
            }

            var totalCount = await notificaionsQuery.CountAsync(cancellationToken);

            var notifications = await notificaionsQuery
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(NotificationMapper.ToResponse()).ToListAsync(cancellationToken);
            return new PaginatedList<NotificationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = notifications,
                TotalCount = totalCount
            };
        }

        public async Task<Result<int>> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
        {
            var count = await dbContext.Notifications.CountAsync(n => n.UserId == userId && (!n.IsRead), cancellationToken);
            return count;
        }

        public async Task<Result<Updated>> MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            var notifications = await dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.MarkAsRead();
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await cache.RemoveByTagAsync($"Notifications:{userId}");

            return Result.Updated;
        }

        public async Task<Result<Updated>> MarkAsReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default)
        {
            var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(
                n => n.Id == notificationId &&
                     n.UserId == userId,
                cancellationToken);

            if (notification is null)
            {
                return Error.NotFound("Notifications.NotFound", "Notification Not found");
            }

            var readNotificationResult = notification.MarkAsRead();
            if (readNotificationResult.IsError)
            {
                return readNotificationResult.TopError;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await cache.RemoveByTagAsync($"Notifications:{userId}");
            return Result.Updated;
        }

        public async Task<Result<Created>> SendNotificationAsync(string userId, string title, string message, NotificationType type, string? data = null, CancellationToken cancellationToken = default)
        {
            var notification = Notification.Create(userId, title, message, type, data);
            if (notification.IsError)
            {
                return notification.TopError;
            }

            dbContext.Notifications.Add(notification.Value);
            await dbContext.SaveChangesAsync(cancellationToken);
            await cache.RemoveByTagAsync($"Notifications:{userId}");
            await hubContext.Clients.User(userId).SendAsync(
                "NotificationReceived",
                notification.Value.ToResponse(),
                cancellationToken);
            return Result.Created;
        }

        public async Task<Result<Created>> SendNotificationToUsersAsync(
            IEnumerable<string> userIds,
            string title,
            string message,
            NotificationType type,
            string? data = null,
            CancellationToken cancellationToken = default)
        {
            var notifications = new List<Notification>();

            foreach (var userId in userIds)
            {
                var notificationResult = Notification.Create(
                    userId,
                    title,
                    message,
                    type,
                    data);

                if (notificationResult.IsError)
                {
                    return notificationResult.TopError;
                }

                notifications.Add(notificationResult.Value);
            }

            dbContext.Notifications.AddRange(notifications);

            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                await hubContext.Clients
                    .User(notification.UserId)
                    .SendAsync(
                        "NotificationReceived",
                        notification.ToResponse(),
                        cancellationToken);
            }

            foreach (var userId in userIds)
            {
                await cache.RemoveByTagAsync(
                    $"Notifications:{userId}");
            }

            return Result.Created;
        }
    }
}
