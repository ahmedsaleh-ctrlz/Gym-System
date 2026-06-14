using Gym.Application.Common.Interfaces;

namespace Gym.Application.UnitTests.Common;

public sealed record TestRequest(string Name);

public sealed record CachedTestRequest(string Name) : ICachedQuery<string>
{
    public string cacheKey => $"test:{Name}";
    public string[] cacheTag => ["tests", Name];
    public TimeSpan cacheDuration => TimeSpan.FromMinutes(5);
}
