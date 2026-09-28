using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Notifications.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Notifications.Queries.GetNotificationsQuery
{
    public sealed record GetNotificationsQuery(
    string UserId,
    int PageNumber,
    int PageSize,
    bool? IsRead) : ICachedQuery<Result<PaginatedList<NotificationResponse>>>
    {
        public string CacheKey =>
        $"Notifications:{UserId}:{PageNumber}:{PageSize}:{GetReadFilter()}";

        public string[] CacheTag =>
            [$"Notifications:{UserId}"];

        public TimeSpan CacheDuration =>
            TimeSpan.FromMinutes(10);

        private string GetReadFilter()
        {
            return IsRead switch
            {
                true => "read",
                false => "unread",
                null => "all"
            };
        }
    }
}
