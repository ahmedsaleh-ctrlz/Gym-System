using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Models;
using Gym.Application.Features.Notifications.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Notifications.Enums;

namespace Gym.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task<Result<Created>> SendNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            string? data = null,
            CancellationToken cancellationToken = default);

        Task<Result<Created>> SendNotificationToUsersAsync(
            IEnumerable<string> userIds,
            string title,
            string message,
            NotificationType type,
            string? data = null,
            CancellationToken cancellationToken = default);

        Task<Result<PaginatedList<NotificationResponse>>> GetNotificationsAsync(
            string userId,
            int pageNumber,
            int pageSize,
            bool? isRead,
            CancellationToken cancellationToken = default);

        Task<Result<Updated>> MarkAsReadAsync(
            int notificationId,
            string userId,
            CancellationToken cancellationToken = default);

        Task<Result<Updated>> MarkAllAsReadAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<Result<int>> GetUnreadCountAsync(
            string userId,
            CancellationToken cancellationToken = default);
            }
}
