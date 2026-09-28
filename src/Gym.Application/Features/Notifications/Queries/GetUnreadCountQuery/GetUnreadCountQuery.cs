using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Notifications.Queries.GetUnreadCountQuery
{
    public sealed record GetUnreadCountQuery(string UserId)
    : ICachedQuery<Result<int>>
    {
        public string CacheKey =>
            $"Notifications:{UserId}:unread-count";

        public string[] CacheTag =>
            [$"Notifications:{UserId}"];

        public TimeSpan CacheDuration =>
            TimeSpan.FromMinutes(5);
    }
}
