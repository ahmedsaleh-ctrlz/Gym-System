using Gym.Application.Common.Interfaces;

namespace Gym.Application.UnitTests.Common;

public sealed record TestRequest(string Name);

public sealed record CachedTestRequest(string Name) : ICachedQuery<string>
{
    public string CacheKey => $"test:{Name}";
    public string[] CacheTag => ["tests", Name];
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}