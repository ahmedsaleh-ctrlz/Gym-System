using Gym.Application.Features.Notifications.Mappers;
using Gym.Domain.Notifications;
using Gym.Domain.Notifications.Enums;
using Gym.Tests.Common.Notifications;
using Gym.Tests.Common.Reflection;

namespace Gym.Application.UnitTests.Mappers;

public class NotificationMapperTests
{
    [Fact]
    public void ToResponse_ExtensionMethod_ShouldMapAllFieldsAccurately()
    {
        var notification = NotificationFactory.CreateNotification(
            userId: "user-abc",
            title: "Expiry Alert",
            message: "Your subscription is expiring soon.",
            type: NotificationType.MembershipExpiring,
            data: "{ \"daysLeft\": 3 }").Value;

        ReflectionTestHelper.SetProperty(notification, nameof(Notification.Id), 77);

        var response = notification.ToResponse();

        Assert.Equal(77, response.Id);
        Assert.Equal("Expiry Alert", response.Title);
        Assert.Equal("Your subscription is expiring soon.", response.Message);
        Assert.Equal(NotificationType.MembershipExpiring, response.Type);
        Assert.Equal("{ \"daysLeft\": 3 }", response.Data);
        Assert.False(response.IsRead);
        Assert.Equal(notification.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public void ToResponse_Expression_ShouldCompileAndMapProperties()
    {
        var notification = NotificationFactory.CreateNotification().Value;
        ReflectionTestHelper.SetProperty(notification, nameof(Notification.Id), 101);

        var compiledExpr = NotificationMapper.ToResponse().Compile();
        var response = compiledExpr(notification);

        Assert.Equal(101, response.Id);
        Assert.Equal(notification.Title, response.Title);
        Assert.Equal(notification.Message, response.Message);
        Assert.Equal(notification.Type, response.Type);
        Assert.Equal(notification.IsRead, response.IsRead);
    }
}
