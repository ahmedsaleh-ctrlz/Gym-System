using Gym.Domain.Common.Result;

namespace Gym.Domain.Coachs;

public static class CoachError
{
    public static Error InvalidHireDate => Error.Validation("Invalid_Hire_Date", "HireDateCannotBeInTheFuture");
    public static Error CannotUpdateInactiveCoach => Error.Conflict("Cannot_Update_Inactive_Coach", "CannotUpdateInactiveCoach");
    public static Error CoachAlreadyActive => Error.Conflict("Coach_Already_Active", "CoachAlreadyActive");
    public static Error CoachAlreadyInactive => Error.Conflict("Coach_Already_Inactive", "CoachAlreadyInactive");
}
