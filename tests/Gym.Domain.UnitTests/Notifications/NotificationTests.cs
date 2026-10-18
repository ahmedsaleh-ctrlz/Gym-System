using Gym.Domain.Notifications;
using Gym.Domain.Notifications.Enums;
using Gym.Tests.Common.Notifications;

namespace Gym.Domain.UnitTests.Notifications;

public class NotificationTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_ShouldReturnError_WhenTitleIsEmptyOrNull(string? invalidTitle)
    {
        var result = Notification.Create("user-1", invalidTitle!, "Valid message", NotificationType.General);

        Assert.True(result.IsError);
        Assert.Equal(NotificationError.TitleOrMessageEmpty.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_ShouldReturnError_WhenMessageIsEmptyOrNull(string? invalidMessage)
    {
        var result = Notification.Create("user-1", "Valid Title", invalidMessage!, NotificationType.General);

        Assert.True(result.IsError);
        Assert.Equal(NotificationError.TitleOrMessageEmpty.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WithCorrectInitialState_WhenInputsAreValid()
    {
        var before = DateTime.UtcNow;
        var result = Notification.Create("user-1", "Payment Received", "Your payment of 500 was processed.", NotificationType.Payment, "{ \"amount\": 500 }");

        Assert.True(result.IsSuccess);
        var notification = result.Value;
        Assert.Equal("user-1", notification.UserId);
        Assert.Equal("Payment Received", notification.Title);
        Assert.Equal("Your payment of 500 was processed.", notification.Message);
        Assert.Equal(NotificationType.Payment, notification.Type);
        Assert.Equal("{ \"amount\": 500 }", notification.Data);
        Assert.False(notification.IsRead);
        Assert.Null(notification.ReadAt);
        Assert.True(notification.CreatedAt >= before.AddSeconds(-1));
    }

    [Fact]
    public void MarkAsRead_ShouldReturnSuccess_AndSetReadState_WhenNotificationIsUnread()
    {
        var notification = NotificationFactory.CreateNotification().Value;
        var before = DateTime.UtcNow;

        var result = notification.MarkAsRead();

        Assert.True(result.IsSuccess);
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
        Assert.True(notification.ReadAt >= before.AddSeconds(-1));
    }

    [Fact]
    public void MarkAsRead_ShouldReturnError_WhenNotificationIsAlreadyRead()
    {
        var notification = NotificationFactory.CreateNotification().Value;
        notification.MarkAsRead();

        var secondResult = notification.MarkAsRead();

        Assert.True(secondResult.IsError);
        Assert.Equal(NotificationError.MessageAlreadyReaded.Code, secondResult.TopError.Code);
    }
}
