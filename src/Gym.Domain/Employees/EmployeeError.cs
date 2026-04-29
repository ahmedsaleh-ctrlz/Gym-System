using Gym.Domain.Common.Result;

namespace Gym.Domain.Employees;

public static class EmployeeError
{
    public static Error InvalidHireDate => Error.Validation("Invalid_Hire_Date", "HireDateCannotBeInTheFuture");
    public static Error CannotUpdateInactiveEmployee => Error.Conflict("Cannot_Update_Inactive_Employee", "CannotUpdateInactiveEmployee");
    public static Error EmployeeAlreadyActive => Error.Conflict("Employee_Already_Active", "EmployeeAlreadyActive");
    public static Error EmployeeAlreadyInactive => Error.Conflict("Employee_Already_Inactive", "EmployeeAlreadyInactive");
}
