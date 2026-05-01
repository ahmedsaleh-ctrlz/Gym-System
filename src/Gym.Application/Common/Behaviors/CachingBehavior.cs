using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result.Abstraction;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger) 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(ct);
        }

        logger.LogInformation("Checking cache for request of type {RequestType}", typeof(TRequest).Name);

        var result = await cache.GetOrCreateAsync<TResponse>(cachedRequest.cacheKey, _ => new ValueTask<TResponse>((TResponse)(object)null!),
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData
            }, cancellationToken: ct);

        if (result is null)
        {
            result = await next(ct);

            if (result is IResult res && res.IsSuccess)
            {
                logger.LogInformation("Caching result for {RequestName}", typeof(TRequest).Name);

                await cache.SetAsync(
                    cachedRequest.cacheKey,
                    result,
                    new HybridCacheEntryOptions
                    {
                        Expiration = cachedRequest.cacheDuration
                    },
                    [cachedRequest.cacheTag],
                    ct);
            }
        }

        return result;
       
    }
}
