using Gym.Domain.Common.Result;

namespace Gym.Domain.Plans;

public static class PlanError
{
    public static Error TitleRequired => Error.Validation("Plan_Title_Required", "PlanTitleRequired");
    public static Error InvalidCost => Error.Validation("Plan_Cost_Invalid", "PlanCostInvalid");
    public static Error CannotUpdateInactivePlan => Error.Conflict("Cannot_Update_Inactive_Plan", "CannotUpdateInactivePlan");
    public static Error PlanAlreadyActive => Error.Conflict("Plan_Already_Active", "PlanAlreadyActive");
    public static Error PlanAlreadyInactive => Error.Conflict("Plan_Already_Inactive", "PlanAlreadyInactive");
    public static Error PlanInactive => Error.Conflict("Plan_Inactive", "PlanInactive");
}
