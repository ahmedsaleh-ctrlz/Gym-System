namespace Gym.Api.Contracts.Identity
{
    public sealed record RefreshTokenRequest(string RefreshToken, string ExpiredAccessToken);
}
