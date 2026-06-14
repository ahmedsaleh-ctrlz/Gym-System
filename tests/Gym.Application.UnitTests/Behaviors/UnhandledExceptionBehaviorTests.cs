using Gym.Application.Common.Behaviors;
using Gym.Application.UnitTests.Common;
using Microsoft.Extensions.Logging;

namespace Gym.Application.UnitTests.Behaviors;

public class UnhandledExceptionBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnResponse_WhenNoExceptionOccurs()
    {
        var logger = new TestLogger<UnhandledExceptionBehavior<TestRequest, string>>();
        var behavior = new UnhandledExceptionBehavior<TestRequest, string>(logger);

        var result = await behavior.Handle(
            new TestRequest("ok"),
            _ => Task.FromResult("done"),
            CancellationToken.None);

        Assert.Equal("done", result);
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task Handle_ShouldLogAndRethrow_WhenExceptionOccurs()
    {
        var logger = new TestLogger<UnhandledExceptionBehavior<TestRequest, string>>();
        var behavior = new UnhandledExceptionBehavior<TestRequest, string>(logger);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestRequest("boom"),
                _ => throw new InvalidOperationException("failure"),
                CancellationToken.None));

        Assert.Equal("failure", exception.Message);
        Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, logger.Entries[0].LogLevel);
        Assert.Same(exception, logger.Entries[0].Exception);
    }
}
