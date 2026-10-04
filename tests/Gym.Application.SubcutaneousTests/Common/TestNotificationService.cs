using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Notifications.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Notifications.Enums;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class TestNotificationService : INotificationService
{
    public Task<Result<PaginatedList<NotificationResponse>>> GetNotificationsAsync(
    string userId,
    int pageNumber,
    int pageSize,
    bool? isRead,
    CancellationToken cancellationToken = default)
    {
        var result = new PaginatedList<NotificationResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Items = [],
            TotalCount = 0
        };

        return Task.FromResult<Result<PaginatedList<NotificationResponse>>>(result);
    }

    public Task<Result<int>> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<int>>(0);
    }

    public Task<Result<Updated>> MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<Updated>>(Result.Updated);
    }

    public Task<Result<Updated>> MarkAsReadAsync(
        int notificationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<Updated>>(Result.Updated);
    }

    public Task<Result<Created>> SendNotificationAsync(
        string userId,
        string title,
        string message,
        NotificationType type,
        string? data = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<Created>>(Result.Created);
    }

    public Task<Result<Created>> SendNotificationToUsersAsync(
        IEnumerable<string> userIds,
        string title,
        string message,
        NotificationType type,
        string? data = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<Created>>(Result.Created);
    }
}