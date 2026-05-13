using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Subscriptions.Enums;

public enum SubscriptionStatus
{
    Pending,
    Active, 
    Expired,
    Frozen,
    Cancelled
}
