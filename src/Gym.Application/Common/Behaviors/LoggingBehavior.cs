
using Gym.Application.Common.Interfaces;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Common.Behaviors;

public class LoggingBehavior<TRequest>(ILogger<LoggingBehavior<TRequest>> logger, IUser user, IIdentityService identityService) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        int? userId = user.Id;
        var userName = string.Empty;

        if (userId is not null)
        {
            userName = await identityService.GetUserNameByIdAsync(userId.Value,cancellationToken);
        }

        logger.LogInformation(
            "Request: {Name} {@UserId} {@UserName} {@Request}", requestName, userId, userName, request);
    }
}