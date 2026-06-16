using Gym.Application.Common.Behaviors;
using Gym.Application.Common.Interfaces;
using Gym.Application.UnitTests.Common;

using NSubstitute;

namespace Gym.Application.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    [Fact]
    public async Task Process_ShouldLogRequest_WithoutCallingIdentityService_WhenUserIdIsMissing()
    {
        var logger = new TestLogger<LoggingBehavior<TestRequest>>();
        var user = Substitute.For<IUser>();
        var identityService = Substitute.For<IIdentityService>();
        user.Id.Returns((string?)null);
        var behavior = new LoggingBehavior<TestRequest>(logger, user, identityService);

        await behavior.Process(new TestRequest("plans"), CancellationToken.None);

        await identityService.DidNotReceiveWithAnyArgs().GetUserNameByIdAsync(default!, default);
        Assert.Single(logger.Entries);
        Assert.Contains("TestRequest", logger.Entries[0].Message);
    }

    [Fact]
    public async Task Process_ShouldResolveUserName_AndLogRequest_WhenUserIdExists()
    {
        var logger = new TestLogger<LoggingBehavior<TestRequest>>();
        var user = Substitute.For<IUser>();
        var identityService = Substitute.For<IIdentityService>();
        user.Id.Returns("user-1");
        identityService.GetUserNameByIdAsync("user-1", Arg.Any<CancellationToken>()).Returns("Ahmed");
        var behavior = new LoggingBehavior<TestRequest>(logger, user, identityService);

        await behavior.Process(new TestRequest("plans"), CancellationToken.None);

        await identityService.Received(1).GetUserNameByIdAsync("user-1", Arg.Any<CancellationToken>());
        Assert.Single(logger.Entries);
        Assert.Contains("Ahmed", logger.Entries[0].Message);
    }
}