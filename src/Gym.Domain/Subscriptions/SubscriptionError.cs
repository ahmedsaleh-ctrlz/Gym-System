using Gym.Domain.Common.Result;

namespace Gym.Domain.Subscriptions;

public static class SubscriptionError
{
    public static Error MemberRequired => Error.Validation("Subscription_Member_Required", "SubscriptionMemberRequired");
    public static Error MemberDeleted => Error.Conflict("Subscription_Member_Deleted", "SubscriptionMemberDeleted");
    public static Error PlanRequired => Error.Validation("Subscription_Plan_Required", "SubscriptionPlanRequired");
    public static Error PlanInactive => Error.Conflict("Subscription_Plan_Inactive", "SubscriptionPlanInactive");
    public static Error InvalidDuration => Error.Validation("Subscription_Duration_Invalid", "SubscriptionDurationInvalid");
    public static Error InvalidOriginalPrice => Error.Validation("Subscription_Original_Price_Invalid", "SubscriptionOriginalPriceInvalid");
    public static Error InvalidDiscountAmount => Error.Validation("Subscription_Discount_Amount_Invalid", "SubscriptionDiscountAmountInvalid");
    public static Error InvalidFinalPrice => Error.Validation("Subscription_Final_Price_Invalid", "SubscriptionFinalPriceInvalid");
    public static Error InvalidStatus => Error.Validation("Subscription_Status_Invalid", "SubscriptionStatusInvalid");
}
