using Gym.Application.Features.Subscriptions.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Subscriptions.Queries.GetMemberSubscriptions
{
    public sealed record GetMemberSubscriptionsQuery(int MemberId) : IRequest<Result<List<SubscriptionResponse>>>;
}