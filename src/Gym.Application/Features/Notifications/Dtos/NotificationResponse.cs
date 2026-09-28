using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Gym.Domain.Notifications;
using Gym.Domain.Notifications.Enums;

namespace Gym.Application.Features.Notifications.Dtos
{
    public sealed record NotificationResponse(
    int Id,
    string Title,
    string Message,
    NotificationType Type,
    string? Data,
    bool IsRead,
    DateTime CreatedAt);
}
