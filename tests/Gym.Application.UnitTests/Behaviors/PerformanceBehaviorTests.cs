using Gym.Application.Common.Behaviors;
using Gym.Application.Common.Interfaces;
using Gym.Application.UnitTests.Common;
using NSubstitute;

namespace Gym.Application.UnitTests.Behaviors;

public class PerformanceBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnResponse_WithoutWarning_WhenRequestIsFast()
    {
        var logger = new TestLogger<TestRequest>();
        var user = Substitute.For<IUser>();
        var identityService = Substitute.For<IIdentityService>();
        var behavior = new PerformanceBehavior<TestRequest, string>(logger, user, identityService);

        var result = await behavior.Handle(
            new TestRequest("fast"),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Empty(logger.Entries);
        await identityService.DidNotReceiveWithAnyArgs().GetUserNameByIdAsync(default!, default);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WithoutResolvingUser_WhenRequestIsSlow_AndUserIdMissing()
    {
        var logger = new TestLogger<TestRequest>();
        var user = Substitute.For<IUser>();
        var identityService = Substitute.For<IIdentityService>();
        user.Id.Returns((string?)null);
        var behavior = new PerformanceBehavior<TestRequest, string>(logger, user, identityService);

        var result = await behavior.Handle(
            new TestRequest("slow"),
            async _ =>
            {
                await Task.Delay(650);
                return "ok";
            },
            CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Single(logger.Entries);
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Warning, logger.Entries[0].LogLevel);
        Assert.Contains("Long Running Request", logger.Entries[0].Message);
        await identityService.DidNotReceiveWithAnyArgs().GetUserNameByIdAsync(default!, default);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_AndResolveUser_WhenRequestIsSlow_AndUserIdExists()
    {
        var logger = new TestLogger<TestRequest>();
        var user = Substitute.For<IUser>();
        var identityService = Substitute.For<IIdentityService>();
        user.Id.Returns("user-1");
        identityService.GetUserNameByIdAsync("user-1", Arg.Any<CancellationToken>()).Returns("Ahmed");
        var behavior = new PerformanceBehavior<TestRequest, string>(logger, user, identityService);

        var result = await behavior.Handle(
            new TestRequest("slow"),
            async _ =>
            {
                await Task.Delay(650);
                return "ok";
            },
            CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Single(logger.Entries);
        Assert.Contains("Ahmed", logger.Entries[0].Message);
        await identityService.Received(1).GetUserNameByIdAsync("user-1", Arg.Any<CancellationToken>());
    }
}
