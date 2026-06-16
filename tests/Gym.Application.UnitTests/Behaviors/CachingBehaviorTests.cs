using Gym.Application.Common.Behaviors;
using Gym.Application.UnitTests.Common;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Gym.Application.UnitTests.Behaviors;

public class CachingBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCallNextDirectly_WhenRequestIsNotCached()
    {
        var cache = CreateHybridCache();
        var logger = new TestLogger<CachingBehavior<TestRequest, string>>();
        var behavior = new CachingBehavior<TestRequest, string>(cache, logger);
        var callCount = 0;

        var result = await behavior.Handle(
            new TestRequest("members"),
            _ =>
            {
                callCount++;
                return Task.FromResult("result");
            },
            CancellationToken.None);

        Assert.Equal("result", result);
        Assert.Equal(1, callCount);
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task Handle_ShouldUseCache_WhenRequestImplementsCachedQuery()
    {
        var cache = CreateHybridCache();
        var logger = new TestLogger<CachingBehavior<CachedTestRequest, string>>();
        var behavior = new CachingBehavior<CachedTestRequest, string>(cache, logger);
        var callCount = 0;
        var request = new CachedTestRequest("payments");

        var firstResult = await behavior.Handle(
            request,
            _ =>
            {
                callCount++;
                return Task.FromResult("cached-response");
            },
            CancellationToken.None);

        var secondResult = await behavior.Handle(
            request,
            _ =>
            {
                callCount++;
                return Task.FromResult("new-response");
            },
            CancellationToken.None);

        Assert.Equal("cached-response", firstResult);
        Assert.Equal("cached-response", secondResult);
        Assert.Equal(1, callCount);
        Assert.True(logger.Entries.Count >= 2);
    }

    private static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDistributedMemoryCache();
        services.AddHybridCache();

        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }
}