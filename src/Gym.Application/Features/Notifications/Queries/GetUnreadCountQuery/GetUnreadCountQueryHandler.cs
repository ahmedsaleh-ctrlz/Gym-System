using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Notifications.Queries.GetUnreadCountQuery
{
    public sealed class GetUnreadCountQueryHandler(
    INotificationService notificationService)
    : IRequestHandler<GetUnreadCountQuery, Result<int>>
    {
        public Task<Result<int>> Handle(
            GetUnreadCountQuery request,
            CancellationToken cancellationToken)
        {
            return notificationService.GetUnreadCountAsync(
                request.UserId,
                cancellationToken);
        }
    }
}
