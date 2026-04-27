
using Gym.Domain.Common.Result;
using System.Data.SqlTypes;

namespace Gym.Domain.Members;
public static class MemberError
{
    public static Error JoinDataInvalid => Error.Validation("Join_Date_Invalid", "JoinDateInvalid");
    public static Error MemberAlreadyDeleted => Error.Conflict("Member_Already_Deleted", "MemberAlreadyDeleted");

    public static Error CannotUpdateDeletedMember => Error.Conflict("Cannot_Update_Deleted_Member", "CannotUpdateDeletedMember");

}
