using Gym.Application.Features.Notifications.Commands;
using Gym.Application.Features.Notifications.Queries.GetNotificationsQuery;
using Gym.Application.Features.Notifications.Queries.GetUnreadCountQuery;
using Gym.Application.SubcutaneousTests.Common;

namespace Gym.Application.SubcutaneousTests.Features.Notifications;

public class NotificationFeatureTests
{
    [Fact]
    public async Task GetNotificationsQuery_ShouldReturnResultsFromService()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var query = new GetNotificationsQuery(UserId: "test-user-id", PageNumber: 1, PageSize: 10, IsRead: null);
        var result = await context.Mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetUnreadCountQuery_ShouldReturnUnreadCount()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var query = new GetUnreadCountQuery("test-user-id");
        var result = await context.Mediator.Send(query);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 0);
    }

    [Fact]
    public async Task MarkAsReadCommand_ShouldReturnSuccess()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var command = new MarkAsReadCommand(1, "test-user-id");
        var result = await context.Mediator.Send(command);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MarkAllAsReadCommand_ShouldReturnSuccess()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var command = new MarkAllAsReadCommand("test-user-id");
        var result = await context.Mediator.Send(command);

        Assert.True(result.IsSuccess);
    }
}
