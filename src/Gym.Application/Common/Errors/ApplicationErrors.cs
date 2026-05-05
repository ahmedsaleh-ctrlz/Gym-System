using Gym.Domain.Common.Result;

namespace Gym.Application.Common.Errors;

public class ApplicationErrors
{
    public static Error MemberNotFound => Error.NotFound("MemberNotFound", "Member with the specified ID was not found.");
    public static Error CannotDeleteSubscribedMember => Error.Conflict("CannotDeleteSubscribedMember", "Cannot delete a member who is currently subscribed to a plan.");
    public static Error CoachNotFound => Error.NotFound("CoachNotFound", "Coach with the specified ID was not found.");
}
