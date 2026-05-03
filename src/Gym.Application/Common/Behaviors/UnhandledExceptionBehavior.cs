
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Common.Behaviors
{
    public class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex) 
            {
                var requestName = typeof(TRequest).Name;
                logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

                throw;
            }

        }
    }
}
