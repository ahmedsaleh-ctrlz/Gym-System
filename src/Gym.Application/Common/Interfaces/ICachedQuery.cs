
using MediatR;

namespace Gym.Application.Common.Interfaces;

public interface ICachedQuery
{
    public string cacheKey { get; }
    public string cacheTag { get; }

    public TimeSpan cacheDuration { get; }

}

public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;