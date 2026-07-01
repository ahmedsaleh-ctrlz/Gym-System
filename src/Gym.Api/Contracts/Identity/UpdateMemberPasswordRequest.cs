namespace Gym.Api.Contracts.Identity;

public sealed record UpdateMemberPasswordRequest(int MemberId,
    string CurrentPassword,
    string NewPassword);