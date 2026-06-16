using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.Subscriptions.Queries.GetSubscriptionById;

public sealed record GetSubscriptionByMemberIdQuery(int MemberId) : ICachedQuery<Result<SubscriptionResponse>>
{
    public string CacheKey => "subscriptionMemberId_" + MemberId;

    public string[] CacheTag => ["Subscriptions"];

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}