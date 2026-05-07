using Gym.Domain.Common.Result;

namespace Gym.Application.Common.Errors;

public class ApplicationErrors
{
    public static Error MemberNotFound => Error.NotFound("MemberNotFound", "Member with the specified ID was not found.");
    public static Error CannotDeleteSubscribedMember => Error.Conflict("CannotDeleteSubscribedMember", "Cannot delete a member who is currently subscribed to a plan.");
    public static Error CoachNotFound => Error.NotFound("CoachNotFound", "Coach with the specified ID was not found.");
    public static Error PlanNotFound => Error.NotFound("PlanNotFound", "Plan with the specified ID was not found.");

    public static readonly Error ExpiredAccessTokenInvalid = Error.Conflict(
        code: "Auth.ExpiredAccessToken.Invalid",
        description: "Expired access token is not valid.");

    public static readonly Error UserIdClaimInvalid = Error.Conflict(
        code: "Auth.UserIdClaim.Invalid",
        description: "Invalid userId claim.");

    public static readonly Error RefreshTokenExpired = Error.Conflict(
        code: "Auth.RefreshToken.Expired",
        description: "Refresh token is invalid or has expired.");
}
