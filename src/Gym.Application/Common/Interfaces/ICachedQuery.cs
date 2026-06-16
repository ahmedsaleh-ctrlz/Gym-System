using MediatR;

namespace Gym.Application.Common.Interfaces;

public interface ICachedQuery
{
    public string CacheKey { get; }
    public string[] CacheTag { get; }

    public TimeSpan CacheDuration { get; }
}

public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;