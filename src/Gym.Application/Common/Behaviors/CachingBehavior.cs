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

        logger.LogInformation("Cache tags: {Tags}", cachedRequest.CacheTag);
        var response = await cache.GetOrCreateAsync<TResponse>(cachedRequest.CacheKey, async _ =>
        {
            return await next(ct);
        }, new HybridCacheEntryOptions
        {
            Expiration = cachedRequest.CacheDuration
        }, cachedRequest.CacheTag,
        cancellationToken: ct);

        return response;
    }
}