using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Notifications.Enums
{
    public enum NotificationType
    {
        General,
        NewSubscription,
        SubscriptionStatusChanged,
        MembershipExpiring,
        Payment,
        System
    }
}
