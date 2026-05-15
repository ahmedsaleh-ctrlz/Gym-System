using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Domain.Common.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Subscriptions.Queries.GetSubscriptionById;

public sealed record GetSubscriptionByMemberIdQuery(int memberId) : ICachedQuery<Result<SubscriptionResponse>>
{
    public string cacheKey => "subscriptionMemberId_" + memberId;

    public string[] cacheTag => ["Subscriptions"];

    public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
}
