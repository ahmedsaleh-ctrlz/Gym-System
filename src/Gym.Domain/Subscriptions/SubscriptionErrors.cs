using Gym.Domain.Common.Result;


namespace Gym.Domain.Subscriptions;

public static class SubscriptionErrors
{
    public static Error SubscriptionShouldAssignToMember => Error.Conflict("Invalid_MemberId","Invalid Member Id , Subscription must assign to member");

    public static Error SubscriptionShouldAssignToPlan => Error.Conflict("Invalid_PlanId", "Invalid Plan Id , Subscription must assign to plan");
    public static Error SubscriptionStartDateCannotBeInThePast => Error.Conflict("Invalid_StartDate", "Start date cannot be in the past");
    public static Error InvalidStartDate => Error.Conflict("Invalid_StartDate", "Start date must be at least 14 days from today");

    public static Error OnlyPendingSubscriptionsCanBeActivated => Error.Conflict("Invalid_Subscription_Status", "Only pending subscriptions can be activated.");

    public static Error OnlyActiveSubscriptionsCanBeFrozen => Error.Conflict("Invalid_Subscription_Status", "Only active subscriptions can be frozen.");
    public static Error CannotFreezeMoreThanAllowedFreezeCount => Error.Conflict("Invalid_Subscription_Status", "Cannot freeze more than allowed freeze count.");
    
    public static Error CannotFreezeMoreThanAllowedFreezeDays => Error.Conflict("Invalid_Freeze_Days", "Cannot freeze more than allowed freeze days.");

    public static Error OnlyFrozenSubscriptionsCanBeUnfrozen => Error.Conflict("Invalid_Subscription_Status", "Only frozen subscriptions can be unfrozen.");

    public static Error FreezeEndDateNotNow => Error.Conflict("Invalid_Unfreeze_Time", "Cannot unfreeze before freeze end date.");

    public static Error OnlyActiveSubscriptionsCanBeCancelled => Error.Conflict("Invalid_Subscription_Status", "Only active subscriptions can be cancelled.");
}
