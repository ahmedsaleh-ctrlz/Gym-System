using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Domain.Subscriptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Subscriptions.Mappers
{
    public static class SubscriptionMapper
    {
            public static SubscriptionResponse ToDto(this Subscription subscription)
            {
                return new SubscriptionResponse
                {
                    SubscriptionId = subscription.Id,
                    MemberId = subscription.MemberId,
                    MemberName = $"{subscription.Member.Person.FirstName} {subscription.Member.Person.LastName}",
                    PlanName = subscription.Plan!.Title,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    Status = subscription.Status.ToString(),
                    FreezeCountUsed = subscription.FreezeCountUsed,
                    TotalFreezeDaysUsed = subscription.TotalFreezeDaysUsed
                };
        }
    }
}
