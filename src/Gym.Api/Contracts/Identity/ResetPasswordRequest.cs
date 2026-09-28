namespace Gym.Api.Contracts.Identity
{
    public sealed record ResetPasswordRequest(string Email, string ResetToken, string NewPassword);
}
